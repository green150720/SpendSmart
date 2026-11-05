using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpendSmart.Models;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;


namespace SpendSmart.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SpendSmartDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            SpendSmartDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Expenses()
        {
            // Get the logged-in user's ID
            var userId = _userManager.GetUserId(User);
            
            var allExpenses = _context.Expenses
                .Where(expense => expense.UserId == userId)
                .OrderByDescending(expense => expense.Date)
                .ThenByDescending(expense => expense.Id)
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

            var userId = _userManager.GetUserId(User);

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
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Clear ModelState validation for User fields so validation succeeds
            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (!ModelState.IsValid)
            {
                return View("CreateEditExpense", model);
            }

            if (model.Id == 0)
            {
                // Explicitly set the logged-in user ID
                model.UserId = userId;
                _context.Expenses.Add(model);
            }
            else
            {
                var expenseInDb = _context.Expenses
                    .SingleOrDefault(expense => expense.Id == model.Id && expense.UserId == userId);

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
            var userId = _userManager.GetUserId(User);      

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
