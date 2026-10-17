using Microsoft.AspNetCore.Mvc;
using SpendSmart.Models;
using System.Diagnostics;
using System.Linq;


namespace SpendSmart.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SpendSmartDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            SpendSmartDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Expenses()
        {
            var allExpenses = _context.Expenses
                .OrderByDescending(expense => expense.Date)
                .ToList();

            ViewBag.Expenses = allExpenses.Sum(expense => expense.Value);

            ViewBag.MonthlyExpenses = allExpenses
                .Where(expense =>
                    expense.Date.HasValue &&
                    expense.Date.Value.Month == DateTime.Today.Month &&
                    expense.Date.Value.Year == DateTime.Today.Year)
                .Sum(expense => expense.Value);

            ViewBag.TotalTransactions = allExpenses.Count;

            return View(allExpenses);
        }

        public IActionResult CreateEditExpense(int? id)
        {
            if (id == null)
            {
                return View(new Expense { Date = DateTime.Today });
            }

            var expenseInDb = _context.Expenses
                .SingleOrDefault(expense => expense.Id == id);

            if (expenseInDb == null)
            {
                return NotFound();
            }

            return View(expenseInDb);
        }

        [HttpPost]
        public IActionResult CreateEditExpenseForm(Expense model)
        {
            if (!ModelState.IsValid)
            {
                return View("CreateEditExpense", model);
            }

            if (model.Id == 0)
            {
                _context.Expenses.Add(model);
            }
            else
            {
                var expenseInDb = _context.Expenses
                    .SingleOrDefault(expense => expense.Id == model.Id);

                if (expenseInDb == null)
                {
                    return NotFound();
                }

                expenseInDb.Value = model.Value;
                expenseInDb.Description = model.Description;
                expenseInDb.Category = model.Category;
                expenseInDb.Date = model.Date;
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Expenses));
        }

        public IActionResult DeleteExpense(int id)
        {
            var expenseInDb = _context.Expenses
                .SingleOrDefault(expense => expense.Id == id);

            if (expenseInDb == null)
            {
                return NotFound();
            }

            _context.Expenses.Remove(expenseInDb);
            _context.SaveChanges();

            return RedirectToAction(nameof(Expenses));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
    }
}
