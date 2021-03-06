using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ConsejeriaEpics.Models;
using ConsejeriaEpics.Data;
using Newtonsoft.Json;
using System.Dynamic;
using Microsoft.AspNetCore.Http;

namespace ConsejeriaEpics.Controllers
{
    public class ListadoController : Controller
    {
        private readonly ILogger<ListadoController> _logger;
        private readonly ApplicationDbContext _context;

        private List<Requerimiento> listRequerimientos= new List<Requerimiento>();

        private List<Requerimiento> listMostrar= new List<Requerimiento>();

        private List<Categoria> listCategoria = new List<Categoria>();

        public ListadoController(ILogger<ListadoController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
            listRequerimientos=_context.Requerimientos.ToList();
            listCategoria=_context.Categorias.ToList();
        }


        public IActionResult Consejero()
        {
            var status=HttpContext.Session.GetString("State"); 
            if(status!=null){
                var user = JsonConvert.DeserializeObject<Usuario>(HttpContext.Session.GetString("SessionUser"));
                var tipo = user.Tipo;
                if(tipo=="C"){
                    switch(HttpContext.Session.GetString("Tipo")){
                        case "PENDIENTE": Pendiente();
                                            break;
                        case "EN PROCESO": Procesado();
                                            break;
                        case "TERMINADO": Terminado();
                                            break;
                    }
                    return View();
                }else{
                    HttpContext.Session.Clear();
                    return RedirectToAction("Index","Login");
                }
            }else{
                return RedirectToAction("Index","Login");
            }    
        }

        public IActionResult Estudiante()
        {
            var status=HttpContext.Session.GetString("State");
            if(status!=null){
                var user = JsonConvert.DeserializeObject<Usuario>(HttpContext.Session.GetString("SessionUser"));
                var tipo = user.Tipo;
                if(tipo=="E"){
                    dynamic modelo= new ExpandoObject();
                    foreach(Requerimiento req in listRequerimientos){
                        if(req.User_ID==user.ID){
                            listMostrar.Add(req);
                        }
                    }
                    modelo.Categorias=listCategoria;
                    modelo.Mostrar=listMostrar;
                    return View("Estudiante",modelo);
                }else{
                    HttpContext.Session.Clear();
                    return RedirectToAction("Index","Login");
                }
            }else{
                return RedirectToAction("Index","Login");
            }    
        }

        public IActionResult Reportes(){
            var user = JsonConvert.DeserializeObject<Usuario>(HttpContext.Session.GetString("SessionUser"));
            var tipo = user.Tipo;
                if(tipo=="C"){
                    dynamic modelo = new ExpandoObject();
                    var listMostrar= new List<Requerimiento>();
                    modelo.Categorias=listCategoria;
                    modelo.Selected=1;
                    for(int i=0; i<listRequerimientos.Count; i++){
                        if(listRequerimientos[i].Tipo_Req==modelo.Selected){
                            listMostrar.Add(listRequerimientos[i]);
                        }
                    }
                    modelo.Mostrar=listMostrar;
                    return View("Reportes",modelo);
                }else{
                    HttpContext.Session.Clear();
                    return RedirectToAction("Index","Login");
                }

        }


        public IActionResult Pendiente()
        {
            foreach(Requerimiento req in listRequerimientos){
                if(req.Estado=="PENDIENTE"){
                    listMostrar.Add(req);
                }
            }
            dynamic modelo= new ExpandoObject();
            modelo.Titulo="Requerimientos Pendientes";
            modelo.Desc="Visualiza todos los requerimientos que aun no han sido atendidos";
            modelo.Mostrar=listMostrar;
            modelo.Categorias=listCategoria;
            modelo.ReqUser=null;
            return View("Consejero",modelo);
        }

        public IActionResult Procesado()
        {
            List<Requerimiento> listReqUser= new List<Requerimiento>();
            foreach(Requerimiento req in listRequerimientos){
                if(req.Estado=="EN PROCESO"){
                    if(req.Consejero_ID==(JsonConvert.DeserializeObject<Usuario>(HttpContext.Session.GetString("SessionUser"))).ID){
                        listReqUser.Add(req);
                    }else{
                        listMostrar.Add(req);
                    }
                    
                }
            }
            dynamic modelo= new ExpandoObject();
            modelo.Titulo="Requerimientos En Proceso";
            modelo.Desc="Visualiza todos los requerimientos que estan siendo atendidos";
            modelo.ReqUser=listReqUser;
            modelo.Categorias=listCategoria;
            modelo.Mostrar=listMostrar;
            return View("Consejero",modelo);
        }

        public IActionResult Terminado()
        {
            foreach(Requerimiento req in listRequerimientos){
                if(req.Estado=="TERMINADO"){
                    listMostrar.Add(req);
                }
            }
            dynamic modelo= new ExpandoObject();
            modelo.Titulo="Requerimientos Terminados";
            modelo.Desc="Visualiza todos los requerimientos que ya han sido atendidos";
            modelo.Mostrar=listMostrar;
            modelo.Categorias=listCategoria;
            modelo.ReqUser=null;
            return View("Consejero",modelo);
        }


        public IActionResult Seleccionar(Requerimiento req){

            var user = JsonConvert.DeserializeObject<Usuario>(HttpContext.Session.GetString("SessionUser"));
            Requerimiento requerimiento= _context.Requerimientos.Where(r => r.ID == req.ID).FirstOrDefault();
            _context.Remove(requerimiento);
            _context.SaveChanges();
            requerimiento.Consejero_ID= user.ID;  
            requerimiento.Estado= "EN PROCESO";
            _context.Add(requerimiento);         
            _context.SaveChanges();
            return RedirectToAction("Procesado");
        }

        public IActionResult Responder(Requerimiento req){
            Requerimiento requerimiento= _context.Requerimientos.Where(r => r.ID == req.ID).FirstOrDefault();
            _context.Remove(requerimiento);
            _context.SaveChanges();
            requerimiento.Respuesta= req.Respuesta;
            requerimiento.Fecha_Fin = DateTime.Now;
            requerimiento.Estado="TERMINADO";  
            _context.Add(requerimiento);         
            _context.SaveChanges();
            return RedirectToAction("Terminado");
        }

        public IActionResult Editar(Requerimiento req){
            Requerimiento requerimiento= _context.Requerimientos.Where(r => r.ID == req.ID).FirstOrDefault();
            _context.Remove(requerimiento);
            _context.SaveChanges();
            requerimiento.Tipo_Req= req.Tipo_Req;
            requerimiento.Detalle= req.Detalle; 
            _context.Add(requerimiento);         
            _context.SaveChanges();
            return RedirectToAction("Estudiante");
        }

        public IActionResult Filtrar(Categoria cat){
            var listMostrar= new List<Requerimiento>();
            dynamic modelo = new ExpandoObject();
            for(int i=0; i<listRequerimientos.Count; i++){
                if(listRequerimientos[i].Tipo_Req==cat.ID){
                    listMostrar.Add(listRequerimientos[i]);
                }
            }
            modelo.Mostrar=listMostrar;
            modelo.Categorias=listCategoria;
            modelo.Selected=cat.ID;
            return View("Reportes",modelo);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
