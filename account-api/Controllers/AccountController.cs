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

            updatingAccount.OpenDate = updateAccountData.OpenDate;
            updatingAccount.CloseDate = updateAccountData.CloseDate;
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
                OpenDate = newAccountData.OpenDate,
                CloseDate = newAccountData.CloseDate,
                Address = newAccountData.Address,
                SpaceArea = newAccountData.SpaceArea
            };
            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAccount", new { id = newAccount.Id }, newAccount);
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

        private async Task<(bool, List<string>)> ValidateAccount(AccountData accountData, bool isUpdate)
        {
            var messages = new List<string>();
            bool result = true;
            foreach (var property in accountData.GetType().GetProperties())
            {
                var propertyValue = property.GetValue(accountData);
                var propertyType = propertyValue?.GetType();
                if (property.Name == "CloseDate" || isUpdate && property.Name == "AccountNumber")
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

            if (!isUpdate && (await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountData.AccountNumber)) != null)
            {
                messages.Add("Account with same number are exist");
                result = false;
            }

            return (result, messages);
        }
    }
}
