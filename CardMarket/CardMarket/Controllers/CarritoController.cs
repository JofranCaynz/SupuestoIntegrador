using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CardMarket.Data;
using CardMarket.Models;

namespace CardMarket.Controllers
{
    public class CarritoController : Controller
    {
        private readonly MvcTiendaContexto _context;

        public CarritoController(MvcTiendaContexto context)
        {
            _context = context;
        }

        // GET: Carrito
        public async Task<IActionResult> Index()
        {

            string strNumeroPedido = HttpContext.Session.GetString("NumPedido");
            if (strNumeroPedido == null)
            {
                return RedirectToAction("CarritoVacio");
            }
            
            var pedido = _context.Pedidos
                .Include(p => p.Detalles)
                .ThenInclude(p => p.Producto)
                .Where(p => p.Id == Convert.ToInt32(strNumeroPedido)).FirstOrDefault();

            return View(pedido);

        }

        public IActionResult CarritoVacio()
        {
            return View();
        }

        public async Task<IActionResult> MasCantidad(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalle = await _context.Detalles.FindAsync(id);
            detalle.Cantidad = detalle.Cantidad + 1;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(detalle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetallesExists(detalle.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarLinea(int id)
        {
            var detalle = await _context.Detalles.FindAsync(id);
            _context.Detalles.Remove(detalle);
            await _context.SaveChangesAsync();

            Pedido pedido = await _context.Pedidos
                .Include(d => d.Detalles)
                .Where(d => d.Id == detalle.PedidoId)
                .FirstOrDefaultAsync();

            if (pedido.Detalles.Count() == 0)
            {
                // return RedirectToAction(nameof(CarritoVacio));
                _context.Pedidos.Remove(pedido);
                await _context.SaveChangesAsync();
                HttpContext.Session.Clear();
            }
                
                return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPedido(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Pedido pedido = await _context.Pedidos
                .Include(p => p.Detalles)
                .ThenInclude(p => p.Producto)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            //se cambia el estado del pedido a confirmado

            var confirmado = await _context.Estados
            .Where(e => e.Descripcion == "Confirmado")
            .FirstOrDefaultAsync();
            pedido.EstadoId = confirmado.Id;
            pedido.Confirmado = DateTime.Now;
            // recorrer las lineas del pedido y para cada línea del pedido,
            // obtener de la BD el producto al que está haciendo referencia y restarle la cantidad de unidades

            foreach (Detalle detalle in pedido.Detalles)
            {
                var producto = await _context.Productos
                    .Where(e => e.Id == detalle.ProductoId)
                    .FirstOrDefaultAsync();
                producto.Stock = producto.Stock - detalle.Cantidad;

                _context.Update(producto);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pedido);
                    HttpContext.Session.Clear();
                    await _context.SaveChangesAsync();

                    // Al confirmar el pedido se pone la variable de sesion del pedido actual
                    // HttpContext.Session.Remove("NumPedido");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PedidoExists(pedido.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return RedirectToAction(nameof(Index), "Escaparate");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnularPedido(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Pedido pedido = await _context.Pedidos
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            //se cambia el estado del pedido a confirmado

            var anulado = await _context.Estados
            .Where(e => e.Descripcion == "Anulado")
            .FirstOrDefaultAsync();
            pedido.EstadoId = anulado.Id;
            pedido.Anulado = DateTime.Now;
            // recorrer las lineas del pedido y para cada línea del pedido,
            // obtener de la BD el producto al que está haciendo referencia y restarle la cantidad de unidades

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pedido);
                    HttpContext.Session.Remove("NumPedido");
                    await _context.SaveChangesAsync();

                    // Al confirmar el pedido se pone la variable de sesion del pedido actual
                    // HttpContext.Session.Remove("NumPedido");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PedidoExists(pedido.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return RedirectToAction(nameof(Index), "Escaparate");
        }

        private bool DetallesExists(int id)
        {
            return _context.Detalles.Any(p => p.Id == id);
        }

        public async Task<ActionResult> MenosCantidad(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalle = await _context.Detalles.FindAsync(id);
            detalle.Cantidad = detalle.Cantidad - 1;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(detalle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetallesExists(detalle.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Carrito/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Pedidos == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Estado)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        // GET: Carrito/Create
        public IActionResult Create()
        {
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre");
            ViewData["EstadoId"] = new SelectList(_context.Estados, "Id", "Descripcion");
            return View();
        }

        // POST: Carrito/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Fecha,Confirmado,Preparado,Enviado,Cobrado,Devuelto,Anulado,ClienteId,EstadoId")] Pedido pedido)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pedido);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", pedido.ClienteId);
            ViewData["EstadoId"] = new SelectList(_context.Estados, "Id", "Descripcion", pedido.EstadoId);
            return View(pedido);
        }

        // GET: Carrito/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Pedidos == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", pedido.ClienteId);
            ViewData["EstadoId"] = new SelectList(_context.Estados, "Id", "Descripcion", pedido.EstadoId);
            return View(pedido);
        }

        // POST: Carrito/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Fecha,Confirmado,Preparado,Enviado,Cobrado,Devuelto,Anulado,ClienteId,EstadoId")] Pedido pedido)
        {
            if (id != pedido.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pedido);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PedidoExists(pedido.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", pedido.ClienteId);
            ViewData["EstadoId"] = new SelectList(_context.Estados, "Id", "Descripcion", pedido.EstadoId);
            return View(pedido);
        }

        // GET: Carrito/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Pedidos == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Estado)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        // POST: Carrito/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Pedidos == null)
            {
                return Problem("Entity set 'MvcTiendaContexto.Pedidos'  is null.");
            }
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido != null)
            {
                _context.Pedidos.Remove(pedido);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PedidoExists(int id)
        {
          return (_context.Pedidos?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
