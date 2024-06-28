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
    public class DetallesController : Controller
    {
        private readonly MvcTiendaContexto _context;

        public DetallesController(MvcTiendaContexto context)
        {
            _context = context;
        }

        // GET: Detalles
        public async Task<IActionResult> Index()
        {
            var mvcTiendaContexto = _context.Detalles.Include(d => d.Pedido).Include(d => d.Producto);
            return View(await mvcTiendaContexto.ToListAsync());
        }

        // GET: Detalles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Detalles == null)
            {
                return NotFound();
            }

            var detalle = await _context.Detalles
                .Include(d => d.Pedido)
                .Include(d => d.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (detalle == null)
            {
                return NotFound();
            }

            return View(detalle);
        }

        // GET: Detalles/Create
        public IActionResult Create()
        {
            ViewData["PedidoId"] = new SelectList(_context.Pedidos, "Id", "Id");
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Descripcion");
            return View();
        }

        // POST: Detalles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PedidoId,ProductoId,Cantidad,Precio,Descuento")] Detalle detalle)
        {
            if (ModelState.IsValid)
            {
                _context.Add(detalle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PedidoId"] = new SelectList(_context.Pedidos, "Id", "Id", detalle.PedidoId);
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Descripcion", detalle.ProductoId);
            return View(detalle);
        }

        // GET: Detalles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Detalles == null)
            {
                return NotFound();
            }

            var detalle = await _context.Detalles.FindAsync(id);
            if (detalle == null)
            {
                return NotFound();
            }
            ViewData["PedidoId"] = new SelectList(_context.Pedidos, "Id", "Id", detalle.PedidoId);
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Descripcion", detalle.ProductoId);
            return View(detalle);
        }

        // POST: Detalles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PedidoId,ProductoId,Cantidad,Precio,Descuento")] Detalle detalle)
        {
            if (id != detalle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(detalle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetalleExists(detalle.Id))
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
            ViewData["PedidoId"] = new SelectList(_context.Pedidos, "Id", "Id", detalle.PedidoId);
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Descripcion", detalle.ProductoId);
            return View(detalle);
        }

        // GET: Detalles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Detalles == null)
            {
                return NotFound();
            }

            var detalle = await _context.Detalles
                .Include(d => d.Pedido)
                .Include(d => d.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (detalle == null)
            {
                return NotFound();
            }

            return View(detalle);
        }

        // POST: Detalles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Detalles == null)
            {
                return Problem("Entity set 'MvcTiendaContexto.Detalles'  is null.");
            }
            var detalle = await _context.Detalles.FindAsync(id);
            if (detalle != null)
            {
                _context.Detalles.Remove(detalle);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DetalleExists(int id)
        {
          return (_context.Detalles?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
