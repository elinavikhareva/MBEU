using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Data;
using TaskTracker.Models;

namespace TaskTracker.Controllers;

public class TasksController : Controller
{
    private readonly AppDbContext _db;

    public TasksController(AppDbContext db)
    {
        _db = db;
    }

    // GET /Tasks
    public IActionResult Index()
    {
        var tasks =  _db.Tasks
            .OrderBy(t => t.IsDone)                 // невыполненные выше
            .ThenByDescending(t => t.CreatedUtc)    // новые выше
            .ToList();

        return View(tasks);
    }

    // GET /Tasks/Details/5
    public IActionResult Details(int id)
    {
        var task =  _db.Tasks.Find(id);
        if (task == null)
            return NotFound();

        return View(task);
    }

    // GET /Tasks/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST /Tasks/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TaskItem task)
    {
        
        if (!ModelState.IsValid)
            return View(task);

        task.CreatedUtc = DateTime.UtcNow;
        _db.Tasks.Add(task);
       
         _db.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    // GET /Tasks/Edit/5
    public IActionResult Edit(int id)
    {
        var task =  _db.Tasks.Find(id);
        if (task == null)
            return NotFound();

        return View(task);
    }

    // POST /Tasks/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, TaskItem task)
    {
        if (id != task.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(task);

        _db.Update(task);
         _db.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    // GET /Tasks/Delete/5
    public IActionResult Delete(int id)
    {
        var task = _db.Tasks.Find(id);
        if (task == null)
            return NotFound();

        return View(task);
    }

    // POST /Tasks/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var task =  _db.Tasks.Find(id);
        if (task == null)
            return NotFound();

        _db.Tasks.Remove(task);
         _db.SaveChanges();

        return RedirectToAction(nameof(Index));
    }
}
