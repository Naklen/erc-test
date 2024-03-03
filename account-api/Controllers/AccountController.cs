using account_api.Models;
using account_api.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace account_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ApiContext _context;

        public AccountController(ApiContext context)
        {
            _context = context;
        }

        // GET: api/Account
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccounts()
        {
            return await _context.Accounts.ToListAsync();
        }

        // GET: api/Account/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Account>> GetAccount(int id)
        {
            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return NotFound();
            }

            return account;
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<Account>>> GetAccount(
            bool with_residents,
            string open_date,
            string number,
            string address,
            string firstname,
            string lastname,
            string surname)
        {
            var result = _context.Accounts.AsQueryable();

            if (number != null)
                result = result.Where(a => a.AccountNumber.Contains(number));

            if (address != null)
                result = result.Where(a => a.Address.Contains(address));

            DateTime openDate;
            if (DateTime.TryParse(open_date, out openDate))
            {
                result = result.Where(a => a.OpenDate.Date == openDate.Date);
            }

            result = result.Include(a => a.Residents);

            if (with_residents)
                result = result.Where(a => a.Residents.Count() > 0);

            if (firstname != null)
                result = result.Where(a => a.Residents.Any(r => r.Firstname.Contains(firstname)));

            if (lastname != null)
                result = result.Where(a => a.Residents.Any(r => r.Lastname.Contains(lastname)));

            if (surname != null)
                result = result.Where(a => a.Residents.Any(r => r.Surname.Contains(surname)));

            return await result.ToListAsync();
        }

        // PUT: api/Account/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAccount(int id, AccountData updateAccountData)
        {
            if (!AccountExists(id))
                return NotFound();

            var accountCheckResult = await ValidateAccount(updateAccountData, true);
            if (!accountCheckResult.Item1)
                return BadRequest(accountCheckResult.Item2);

            var updatingAccount = await _context.Accounts.FindAsync(id);

            updatingAccount.OpenDate = updateAccountData.OpenDate.Date;
            updatingAccount.CloseDate = updateAccountData.CloseDate.Date;
            updatingAccount.Address = updateAccountData.Address;
            updatingAccount.SpaceArea = updateAccountData.SpaceArea;

            //_context.Entry(newAccount).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Account
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Account>> PostAccount(AccountData newAccountData)
        {
            var accountCheckResult = await ValidateAccount(newAccountData, false);
            if (!accountCheckResult.Item1)
                return BadRequest(accountCheckResult.Item2);

            Account newAccount = new()
            {
                AccountNumber = newAccountData.AccountNumber,
                OpenDate = newAccountData.OpenDate.Date,
                CloseDate = newAccountData.CloseDate.Date,
                Address = newAccountData.Address,
                SpaceArea = newAccountData.SpaceArea
            };
            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAccount", new { id = newAccount.Id }, newAccount);
        }

        [HttpPost("add-residents")]
        public async Task<IActionResult> AddResidentsToAccount(int accountID, List<int> residents_ids)
        {
            var account = await _context.Accounts.Include(a => a.Residents).FirstOrDefaultAsync(a => a.Id == accountID);

            if (account == null)
                return NotFound("Account are not exist");
            if (account.Residents == null)
                account.Residents = new List<Resident>();

            foreach (var residentID in residents_ids.Distinct())
            {
                var resident = await _context.Residents.FindAsync(residentID);
                if (resident != null)
                    account.Residents.Add(resident);
            }

            _context.SaveChanges();

            return NoContent();
        }

        // DELETE: api/Account/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AccountExists(int id)
        {
            return _context.Accounts.Any(e => e.Id == id);
        }

        private async Task<(bool, List<string>)> ValidateAccount(AccountData accountData, bool is_update)
        {
            var messages = new List<string>();
            bool result = true;
            foreach (var property in accountData.GetType().GetProperties())
            {
                var propertyValue = property.GetValue(accountData);
                var propertyType = propertyValue?.GetType();
                if (property.Name == "CloseDate" || is_update && property.Name == "AccountNumber")
                    continue;
                if (propertyValue == null ||
                    propertyType.IsValueType &&
                    propertyValue.Equals(Activator.CreateInstance(propertyType)))
                {
                    messages.Add($"Field {property.Name} are missed or has default ({(propertyValue == null ? "null" : propertyValue)}) value");
                    result = false;
                }
                if (propertyType == typeof(string) && propertyValue.Equals(""))
                {
                    messages.Add($"Field {property.Name} can not be an empty string");
                    result = false;
                }
            }

            if (accountData.CloseDate != DateTime.MinValue && accountData.OpenDate >= accountData.CloseDate)
            {
                messages.Add("The close date must be greater then the open date");
                result = false;
            }

            if (!is_update && (await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountData.AccountNumber)) != null)
            {
                messages.Add("Account with same number are exist");
                result = false;
            }

            return (result, messages);
        }
    }
}
