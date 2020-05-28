using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using s18033_10.DTOs;
using s18033_10.Models;

namespace s18033_10.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly s18033Context _context;

        public EnrollmentsController(s18033Context context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<Enrollment>> EnrollStudent(EnrollStudentRequest request)
        {

            var response = new EnrollStudentResponse();
            var studyId = await _context.Studies
                .Where(s => s.Name == request.Studies)
                .Select(study => study.IdStudy)
                .FirstOrDefaultAsync();

            try
            {
                if (studyId.Equals(null))
                {
                    return BadRequest("Zadane studia nie istnieją.");
                }

                int enrollmentId = 0;
                var existingEnrollmentId = await _context.Enrollment
                    .Where(enrollment => (enrollment.IdStudy == studyId) && (enrollment.Semester == 1))
                    .Select(enrollment => enrollment.IdEnrollment)
                    .FirstOrDefaultAsync();

                if (existingEnrollmentId.Equals(null))
                {
                    var id = await _context.Enrollment
                        .OrderByDescending(e => e.IdEnrollment)
                        .Select(e => e.IdEnrollment)
                        .FirstOrDefaultAsync();

                    var enrollment = new Enrollment
                    {
                        StartDate = DateTime.Now.Date,
                        IdEnrollment = ++id,
                        IdStudy = studyId,
                        Semester = 1
                    };

                    await _context.Enrollment.AddAsync(enrollment);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    enrollmentId = existingEnrollmentId;
                }

                response.Semester = 1;
                response.StartDate = DateTime.Now.Date;


            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

            return Ok(response);
        }

        [HttpPost("promotions")]
        public async Task<ActionResult<Enrollment>> PostPromotion(PromoteStudentRequest request)
        {
            try
            {
                var newEnrollmentId = await _context.Database.ExecuteSqlRawAsync("Exec PromoteStudents '" + request.Studies + "', " + request.Semester + ";");
                var enrollment = await _context.Enrollment
                                    .Where(enrollment => (enrollment.IdEnrollment == newEnrollmentId))
                                    .Select(enrollment => new { enrollment.Semester, enrollment.StartDate })
                                    .FirstOrDefaultAsync();

                var response = new EnrollStudentResponse();
                response.Semester = enrollment.Semester;
                response.StartDate = enrollment.StartDate;

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("Nie wykonano promocji studiów o zadanej nazwie.");
            }
        }

        private bool EnrollmentExists(int id)
        {
            return _context.Enrollment.Any(e => e.IdEnrollment == id);
        }
    }
}
