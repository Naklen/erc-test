using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using account_api;
using account_api.Models;
using account_api.Utils;

namespace account_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResidentController : ControllerBase
    {
        private readonly ApiContext _context;

        public ResidentController(ApiContext context)
        {
            _context = context;
        }

        // GET: api/Resident
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Resident>>> GetResidents()
        {
            return await _context.Residents.ToListAsync();
        }

        // GET: api/Resident/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Resident>> GetResident(int id)
        {
            var resident = await _context.Residents.FindAsync(id);

            if (resident == null)
            {
                return NotFound();
            }

            return resident;
        }

        // PUT: api/Resident/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutResident(int id, ResidentData updateResidentData)
        {
            if (!ResidentExists(id))           
                return NotFound();            

            var residentCheckResult = await ValidateResident(updateResidentData, id);
            if (!residentCheckResult.Item1)
                return BadRequest(residentCheckResult.Item2);

            var updatingResident = await _context.Residents.FindAsync(id);

            updatingResident.DocumentID = updateResidentData.DocumentID;
            updatingResident.Firstname = updateResidentData.Firstname;
            updatingResident.Lastname = updateResidentData.Lastname;
            updatingResident.Surname = updateResidentData.Surname;
            updatingResident.BirthDate  = updateResidentData.BirthDate;           
            
            await _context.SaveChangesAsync();            

            return NoContent();
        }

        // POST: api/Resident
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Resident>> PostResident(ResidentData newResidentData)
        {
            var residentCheckResult = await ValidateResident(newResidentData, 0);
            if (!residentCheckResult.Item1)
                return BadRequest(residentCheckResult.Item2);

            var newResident = new Resident()
            {
                DocumentID = newResidentData.DocumentID,
                Firstname = newResidentData.Firstname,
                Lastname = newResidentData.Lastname,
                Surname = newResidentData.Surname,
                BirthDate = newResidentData.BirthDate
            };

            _context.Residents.Add(newResident);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetResident", new { id = newResident.Id }, newResident);
        }

        // DELETE: api/Resident/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResident(int id)
        {
            var resident = await _context.Residents.FindAsync(id);
            if (resident == null)
            {
                return NotFound();
            }

            _context.Residents.Remove(resident);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ResidentExists(int id)
        {
            return _context.Residents.Any(e => e.Id == id);
        }

        private async Task<(bool, List<string>)> ValidateResident(ResidentData residentData, int updateID)
        {
            var messages = new List<string>();
            bool result = true;
            foreach (var property in residentData.GetType().GetProperties())
            {
                var propertyValue = property.GetValue(residentData);
                var propertyType = propertyValue?.GetType();
                if (property.Name == "Surname")
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

            var sameDocumentIDResident = await _context.Residents.FirstOrDefaultAsync(a => a.DocumentID == residentData.DocumentID);
            if (sameDocumentIDResident != null && sameDocumentIDResident.Id != updateID)
            {
                messages.Add("Resident with same document ID are exist");
                result = false;
            }

            return (result, messages);
        }
    }
}
