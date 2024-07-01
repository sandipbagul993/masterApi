using Master.API.Models;
using Master.API.Models.DTOs;
using Master.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace Master.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CIMSController : ControllerBase
    {
        private readonly MasterDBContext _dbContext;
        private readonly EmailService _emailService;

        public CIMSController(MasterDBContext dbContext, EmailService emailService)
        {
            _dbContext = dbContext;
            _emailService = emailService;
        }

        [HttpGet("GetCompanies")]
        public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
        {
            return await _dbContext.Companies.ToListAsync();
        }

        [HttpPost("SendEmail")]
        public async Task<IActionResult> SendEmail(EmailRequest request)
        {
            await _emailService.SendEmailAsync(request.ToEmail, request.Subject, request.Message);
            return Ok();
        }

        [HttpPost("AddorEditCompanies")]
        public async Task<ActionResult> AddorEditCompanies(Company company)
        {
            var data = await _dbContext.Companies.AsNoTracking().Where(c => c.CompanyId == company.CompanyId).FirstOrDefaultAsync();
            if (data != null)
            {
                _dbContext.Companies.Update(company);
                await _dbContext.SaveChangesAsync();
                return Ok(company);
            }
            else
            {
                _dbContext.Companies.Add(company);
                await _dbContext.SaveChangesAsync();
                return Ok(company);
            }
        }

        [HttpGet("GetHrs")]
        public async Task<ActionResult<IEnumerable<Hr>>> GetHrs()
        {
            return await _dbContext.Hrs.ToListAsync();
        }

        [HttpGet("getHrByCompanyId")]
        public async Task<ActionResult<Hr>> getHrByCompanyId(int id)
        {
            var data = await _dbContext.Hrs.Where(h => h.CompanyId == id).FirstOrDefaultAsync();
            return Ok(data);
        }

        [HttpGet("GetInterviewers")]
        public async Task<ActionResult<IEnumerable<Interviewer>>> GetInterviewers()
        {
            var result = from interviewer in _dbContext.Interviewers
                         join user in _dbContext.Users on interviewer.UserId equals user.UserId

                         join company in _dbContext.Companies on interviewer.CompanyId equals company.CompanyId

                         join hr in _dbContext.Hrs on company.CompanyId equals hr.CompanyId

                         select new
                         {
                             InterviewerId = interviewer.InterviewerId,
                             Name = interviewer.Name,
                             Email = interviewer.Email,
                             Phone = interviewer.Phone,
                             CompanyId = interviewer.CompanyId,
                             CompanyName = company.CompanyName,
                             UserId = interviewer.UserId,
                             Hrid = interviewer.HrId,
                             HrName = hr.HrName
                         };

            return Ok(await result.ToListAsync());
        }

        [HttpPost("AddorEditHr")]
        public async Task<ActionResult> AddorEditHr(Hr hr)
        {
            var data = await _dbContext.Hrs.AsNoTracking().Where(h => h.Hrid == hr.Hrid).FirstOrDefaultAsync();
            if (data != null)
            {
                _dbContext.Hrs.Update(hr);
                await _dbContext.SaveChangesAsync();
                return Ok(hr);
            }
            else
            {
                _dbContext.Hrs.Add(hr);
                await _dbContext.SaveChangesAsync();
                return Ok(hr);
            }
        }

        [HttpDelete("DeleteHr")]
        public async Task<ActionResult> DeleteHr(int id)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var hr = await _dbContext.Hrs.FindAsync(id);
                    if (hr == null)
                    {
                        return NotFound();
                    }

                    // Get all interviewers related to this HR
                    var interviewers = await _dbContext.Interviewers
                        .Where(i => i.HrId == id)
                        .ToListAsync();

                    // Get all interviews related to these interviewers
                    foreach (var interviewer in interviewers)
                    {
                        var interviews = await _dbContext.Interviews
                            .Where(iv => iv.InterviewerId == interviewer.InterviewerId)
                            .ToListAsync();
                        _dbContext.Interviews.RemoveRange(interviews);
                    }

                    // Remove the interviewers
                    _dbContext.Interviewers.RemoveRange(interviewers);

                    // Remove the HR record
                    _dbContext.Hrs.Remove(hr);

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return NoContent();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }
        }


        [HttpGet("GetJobs")]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobs()
        {
            var Companyjobs = await (from companies in _dbContext.Companies
                                     join jobs in _dbContext.Jobs on companies.CompanyId equals jobs.CompanyId
                                     select new
                                     {
                                         CompanyName = companies.CompanyName,
                                         LogoUrl = companies.LogoUrl,
                                         CompanyId = companies.CompanyId,
                                         JobId = jobs.JobId,
                                         JobTitle = jobs.JobTitle,
                                         JobDescription = jobs.JobDescription,

                                     }).ToListAsync();
            return Ok(Companyjobs);
        }

        [HttpGet("GetJobsAsPerJobTitle")]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobsAsPerJobTitle(string? search)
        {
            if (search != null)
            {
                var Companyjobs = await (from companies in _dbContext.Companies
                                         join jobs in _dbContext.Jobs
                                         on companies.CompanyId equals jobs.CompanyId
                                         select new
                                         {
                                             CompanyName = companies.CompanyName,
                                             LogoUrl = companies.LogoUrl,
                                             CompanyId = companies.CompanyId,
                                             JobId = jobs.JobId,
                                             JobTitle = jobs.JobTitle,
                                             JobDescription = jobs.JobDescription,

                                         }).ToListAsync();
                // Filter jobs in memory
                var filteredJobs = Companyjobs
                    .Where(job => job.JobTitle.Contains(search, System.StringComparison.OrdinalIgnoreCase)
                    || job.CompanyName.Contains(search, System.StringComparison.OrdinalIgnoreCase))
                    .ToList();
                return Ok(filteredJobs);
            }
            else
            {
                var Companyjobs = await (from companies in _dbContext.Companies
                                         join jobs in _dbContext.Jobs
                                         on companies.CompanyId equals jobs.CompanyId
                                         select new
                                         {
                                             CompanyName = companies.CompanyName,
                                             LogoUrl = companies.LogoUrl,
                                             CompanyId = companies.CompanyId,
                                             JobId = jobs.JobId,
                                             JobTitle = jobs.JobTitle,
                                             JobDescription = jobs.JobDescription,

                                         }).ToListAsync();
                return Ok(Companyjobs);
            }


        }

        [HttpPost("AddorEditJobs")]
        public async Task<ActionResult> AddorEditJobs(Job job)
        {
            var data = await _dbContext.Jobs.AsNoTracking().Where(c => c.JobId == job.JobId).FirstOrDefaultAsync();
            if (data != null)
            {
                _dbContext.Jobs.Update(job);
                await _dbContext.SaveChangesAsync();
                return Ok(job);
            }
            else
            {
                _dbContext.Jobs.Add(job);
                await _dbContext.SaveChangesAsync();
                return Ok(job);
            }
        }

        [HttpDelete("DeleteJob")]
        public async Task<ActionResult> DeleteJob(int id)
        {
            var data = await _dbContext.Jobs.Where(i => i.JobId == id).FirstOrDefaultAsync();
            if (data == null)
            {
                return NotFound("Invalid Id");
            }
            _dbContext.Jobs.Remove(data);
            await _dbContext.SaveChangesAsync();
            return Ok(new
            {
                Result = true,
                Meassage = $"JobId {data.JobId} is deleted successfully",
                data = new
                {
                    JobId = data.JobId,
                    JobTitle = data.JobTitle,
                }

            });
        }


        [HttpPost("AddorEditApplications")]
        public async Task<ActionResult> AddorEditApplications(Application application)
        {
            var data = await _dbContext.Applications.AsNoTracking().Where(a => a.ApplicationId == application.ApplicationId).FirstOrDefaultAsync();
            if (data != null)
            {
                _dbContext.Applications.Update(application);
                await _dbContext.SaveChangesAsync();
                return Ok(application);
            }
            else
            {
                _dbContext.Applications.Add(application);
                await _dbContext.SaveChangesAsync();
                return Ok(application);
            }
        }

        [HttpGet("GetApplications")]
        public async Task<ActionResult<IEnumerable<Application>>> GetApplications()
        {
            var data = await (from jobs in _dbContext.Jobs
                              join applications in _dbContext.Applications on jobs.JobId equals applications.JobId
                              join users in _dbContext.Users on applications.UserId equals users.UserId
                              select new
                              {
                                  JobId = applications.JobId,
                                  UserId = users.UserId,
                                  ApplicationId = applications.ApplicationId,
                                  JobTitle = jobs.JobTitle,
                                  StudentName = users.FullName,
                                  Status = applications.Status
                              }).ToListAsync();
            return Ok(data);
        }


        [HttpPost("AddorEditInterviwer")]
        public async Task<ActionResult> AddorEditInterviwer(Interviewer interviewer)
        {
            var data = await _dbContext.Interviewers.AsNoTracking().Where(i => i.InterviewerId == interviewer.InterviewerId).FirstOrDefaultAsync();
            if (data != null)
            {
                _dbContext.Interviewers.Update(interviewer);
                await _dbContext.SaveChangesAsync();
                return Ok(interviewer);
            }
            else
            {
                _dbContext.Interviewers.Add(interviewer);
                await _dbContext.SaveChangesAsync();
                return Ok(interviewer);
            }
        }



        [HttpDelete("DeleteInterviwer")]
        public async Task<ActionResult> DeleteInterviwer(int id)
        {
            // Example using Entity Framework
            var data = _dbContext.Interviewers.Include(i => i.Interviews).FirstOrDefault(i => i.InterviewerId == id);
            if (data != null)
            {
                // Remove all related interviews
                _dbContext.Interviews.RemoveRange(data.Interviews);
                _dbContext.Interviewers.Remove(data);
                await _dbContext.SaveChangesAsync();
            }

            return Ok(new
            {
                InterviewerId = data!.InterviewerId,
                UserId = data.UserId
            });
        }


        [HttpPost("AddorEditInterview")]
        public async Task<ActionResult> AddorEditInterview(Interview interview)
        {
            var data = await _dbContext.Interviews.AsNoTracking().Where(i => i.InterviewId == interview.InterviewId).FirstOrDefaultAsync();
            if (data != null)
            {
                _dbContext.Interviews.Update(interview);
                await _dbContext.SaveChangesAsync();
                return Ok(interview);
            }
            else
            {
                _dbContext.Interviews.Add(interview);
                await _dbContext.SaveChangesAsync();
                return Ok(interview);
            }
        }

        [HttpGet("GetInterviews")]
        public async Task<ActionResult<IEnumerable<Interview>>> GetInterviews()
        {
            var data = await (from applications in _dbContext.Applications
                              join interviews in _dbContext.Interviews on applications.ApplicationId equals interviews.ApplicationId
                              join interviewrs in _dbContext.Interviewers on interviews.InterviewerId equals interviewrs.InterviewerId
                              join jobs in _dbContext.Jobs on applications.JobId equals jobs.JobId
                              join user in _dbContext.Users on applications.UserId equals user.UserId
                              select new
                              {
                                  ApplicationId = interviews.ApplicationId,
                                  JobTitle = jobs.JobTitle,
                                  Name = interviewrs.Name,
                                  InterviewerId = interviews.InterviewerId,
                                  Date = interviews.Date,
                                  Feedback = interviews.Feedback,
                                  InterviewId = interviews.InterviewId,
                                  StudentName = user.FullName,
                                  Email = user.Email,
                                  UserId = interviewrs.UserId
                              }).ToListAsync();
            return Ok(data);
        }

        [HttpGet("GetSelected")]
        public async Task<ActionResult> GetSelectedStudent()
        {
            var data = await (from applications in _dbContext.Applications
                              join interviews in _dbContext.Interviews.Where(i => i.Feedback == "Selected") on applications.ApplicationId equals interviews.ApplicationId
                              join interviewrs in _dbContext.Interviewers on interviews.InterviewerId equals interviewrs.InterviewerId
                              join jobs in _dbContext.Jobs on applications.JobId equals jobs.JobId
                              join user in _dbContext.Users on applications.UserId equals user.UserId
                              select new
                              {
                                  ApplicationId = interviews.ApplicationId,
                                  JobTitle = jobs.JobTitle,
                                  Name = interviewrs.Name,
                                  InterviewerId = interviews.InterviewerId,
                                  Date = interviews.Date,
                                  Feedback = interviews.Feedback,
                                  InterviewId = interviews.InterviewId,
                                  StudentName = user.FullName,
                                  Email = user.Email

                              }).ToListAsync();
            return Ok(data);
        }

        [HttpDelete("DeleteInterviews")]
        public async Task<ActionResult> DeleteInterviews(int id)
        {
            var data = await _dbContext.Interviews.Where(i => i.InterviewId == id).FirstOrDefaultAsync();
            if (data == null)
            {
                return NotFound("Invalid Id");
            }
            _dbContext.Interviews.Remove(data);
            await _dbContext.SaveChangesAsync();
            return Ok(new
            {
                InterviewId = data.InterviewId,
                InterviewerId = data.InterviewerId,

            });
        }


        [HttpPost("AddorEditStudent")]
        public async Task<ActionResult> AddorEditStudent(Student student)
        {
            var data = await _dbContext.Students.AsNoTracking().Where(s => s.StudentId == student.StudentId).FirstOrDefaultAsync();
            if (data != null)
            {
                _dbContext.Students.Update(student);
                await _dbContext.SaveChangesAsync();
                return Ok(student);
            }
            else
            {
                _dbContext.Students.Add(student);
                await _dbContext.SaveChangesAsync();
                return Ok(student);
            }
        }

        [HttpGet("GetStudents")]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            var data = await (from students in _dbContext.Students
                              join users in _dbContext.Users on students.UserId equals users.UserId
                              select new
                              {
                                  StudentName = users.FullName,
                                  Email = users.Email,
                                  Mobile = students.Mobile,
                                  StudentId = students.StudentId,
                                  UserId = students.UserId,
                                  CollegeName = students.CollegeName,
                                  University = students.University,
                                  City = students.City,
                                  Graduation = students.Graduation,
                                  Branch = students.Branch,
                                  GraduationYear = students.GraduationYear,

                              }
                             ).ToListAsync();
            return Ok(data);
        }

        [HttpDelete("DeleteStudent")]
        public async Task<ActionResult> DeleteStudent(int id)
        {
            var data = await _dbContext.Students.Where(s => s.StudentId == id).FirstOrDefaultAsync();
            if (data == null)
            {
                return NotFound("Invalid Id");
            }
            else
            {
                _dbContext.Students.Remove(data);
                await _dbContext.SaveChangesAsync();
                return Ok(data.StudentId);
            }


        }

    }




}
