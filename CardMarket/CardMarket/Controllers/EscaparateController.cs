using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CardMarket.Data;
using CardMarket.Models;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace CardMarket.Controllers
{
    public class EscaparateController : Controller
    {
        private readonly MvcTiendaContexto _context;

        public EscaparateController(MvcTiendaContexto context)
        {
            _context = context;
        }

        // GET: Escaparate
        [Authorize(Roles = "Usuario")]

        public async Task<IActionResult> Index(int? id)
        {
            var productos = _context.Productos.AsQueryable();

            if (id == null)
            {
                // Selecciona productos del escaparte
                productos = productos.Where(x => x.Escaparate == true);
            }
            else
            {
                // Selecciona productos del escaprate
                productos = productos.Where(x => x.CategoriaId == id);

                // Obtiene el nombre de la categoría selecionada
                ViewBag.DescriptionCategoria = _context.Categorias.Find(id).Descripcion.ToString();
            }

            ViewData["ListaCategorias"] = _context.Categorias.OrderBy(c => c.Descripcion).ToList();
            productos = productos.Include(a => a.Categoria);

            return View(await productos.ToListAsync());
        }
        // GET AgregarCarrito
        // La acción GET mostrará los datos del producto a añadir
        public async Task<IActionResult> AgregarCarrito(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }


        // POST: Escaparate/AgregarCarrito/5 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarCarrito(int id)
        {
            string email = User.Identity.Name;

            var usuario = await _context.Clientes
                .Where(e => e.Email == email)
                .FirstOrDefaultAsync();

            var estado = await _context.Estados
                .Where(e => e.Descripcion == "Pendiente")
                .FirstOrDefaultAsync();

            // Cargar datos de producto a añadir al carrito
            var producto = await _context.Productos
            .FirstOrDefaultAsync(m => m.Id == id);



            if (producto == null)
            {
                return NotFound();
            }

            // Crear nuevo pedido, si el carrito está vacío y, por tanto, no existe pedido actual
            // La variable de sesión NumPedido almacena el número de pedido del carrito
            //if (string.IsNullOrEmpty(HttpContext.Session.GetString("NumPedido")) )
            if (HttpContext.Session.GetString("NumPedido") == null)
            {
                // Crear objeto pedido a agregar
                Pedido pedido = new Pedido();
                pedido.Fecha = DateTime.Now;
                pedido.Confirmado = null;
                pedido.Preparado = null;
                pedido.Enviado = null;
                pedido.Cobrado = null;
                pedido.Devuelto = null;
                pedido.Anulado = null;
                pedido.ClienteId = usuario.Id; // Asignar el cliente correspondiente al usuario actual
                                      // Pruebas sobre el cliente Id=2 
                pedido.EstadoId = estado.Id; // Estado: "Pendiente" (Sin confirmar)
                if (ModelState.IsValid)
                {
                    _context.Add(pedido);
                    await _context.SaveChangesAsync();
                }
                // Se asigna el número de pedido a la variable de sesión 
                // que almacena el número de pedido del carrito
                HttpContext.Session.SetString("NumPedido", pedido.Id.ToString());
            }

            var detalles = await _context.Detalles
                .Where(d => d.PedidoId == Convert.ToInt32(HttpContext.Session.GetString("NumPedido")))
                .Where(d => d.ProductoId == producto.Id)
                .FirstOrDefaultAsync();
            // Crear objeto detalle para agregar el producto al detalle del pedido del carrito
            if (detalles == null)
            {
                Detalle detalle = new Detalle();
                string strNumeroPedido = HttpContext.Session.GetString("NumPedido");
                detalle.PedidoId = Convert.ToInt32(strNumeroPedido);
                detalle.ProductoId = id; // El valor id tiene el id del producto a agregar
                detalle.Cantidad = 1;
                detalle.Precio = producto.Precio;
                detalle.Descuento = 0;
                if (ModelState.IsValid)
                {
                    _context.Add(detalle);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                detalles.Cantidad++;
                await _context.SaveChangesAsync();
            }

            
            


            return RedirectToAction(nameof(Index));
        }

        
    }
}
