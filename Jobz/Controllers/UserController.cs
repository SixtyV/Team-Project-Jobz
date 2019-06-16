using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using Jobz.Models;
using System.Web.Security;
using System.IO;
using System.IO.Compression;



namespace Jobz.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private static List<JobListModel> ListJobCategories = new List<JobListModel>();
        private static List<JobListModel> ListJobRegions = new List<JobListModel>();
        private static List<JobListModel> ListWorkHours = new List<JobListModel>();


        public bool FillStaticLists()
        {
            bool ConnectionSuccesful = true;
            DbConnection connection = new DbConnection();
            JobModel job = new JobModel();

            if (ListJobCategories.Count.Equals(0))
                ListJobCategories.AddRange(connection.LoadJobDropDownTablesIntoList("Category", out ConnectionSuccesful));

            if (ListJobRegions.Count.Equals(0) && ConnectionSuccesful)
                ListJobRegions.AddRange(connection.LoadJobDropDownTablesIntoList("Region", out ConnectionSuccesful));

            if (ListWorkHours.Count.Equals(0) && ConnectionSuccesful)
                ListWorkHours.AddRange(connection.LoadJobDropDownTablesIntoList("WorkHours", out ConnectionSuccesful));

            return ConnectionSuccesful;
        }

        [AllowAnonymous]
        public ActionResult CreateUserAccount(int Role = -1)
        {
            if((Role < 0)  || (Role > 1))
                return RedirectToAction("Index", "Home");

            ViewBag.Role = (UserRole) Role;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult CreateUserAccount(UserModel user)
        {
            DbConnection connection = new DbConnection();
            bool UserNameIsLegit;
            bool ConnectionSuccesful = connection.CreateNewAccount(user, out UserNameIsLegit);

            if (!ConnectionSuccesful)
            {
                ModelState.AddModelError("", "An error has occured while trying to connect. Please try again later.");
                return View(user);
            }

            if (!UserNameIsLegit)
            {
                ModelState.AddModelError("UserName", "User Name already exists. Try another one.");
                return View(user);
            }
            return RedirectToAction("Index", "Home");
        }
        
        public ActionResult EditUserAccount()
        {
            DbConnection connection = new DbConnection();
            UserModel user = new UserModel();
            bool ConnectionSuccesful;
            int Id = connection.GetLoggedInUserIdBasedOnUsername(out ConnectionSuccesful);

            if (ConnectionSuccesful)
                user = connection.UserEditAccountGet(Id, out ConnectionSuccesful);

            if ((!ConnectionSuccesful) || (user == null) || (User.Identity.Name != user.UserName))
                return RedirectToAction("Fail", "User");

            return View(user);
        }

        [HttpPost]
        public ActionResult EditUserAccount(UserModel user)
        {
            DbConnection connection = new DbConnection();
            bool UserNameIsLegit;
            bool ConnectionSuccesful = connection.UserEditAccountPost(user, out UserNameIsLegit);

            if (!ConnectionSuccesful)
            {
                ModelState.AddModelError("", "An error has occured while trying to connect. Please try again later.");
                return View(user);
            }

            if (!UserNameIsLegit)
            {
                ModelState.AddModelError("UserName", "User Name already exists. Try another one.");
                return View(user);
            }
            return RedirectToAction("Logout");
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(UserLoginModel LoginUser)
        {
            DbConnection connection = new DbConnection();
            bool ConnectionSuccesful;

            if (!ModelState.IsValid)
                return View(LoginUser);
            
            var user = connection.LogInUser(LoginUser.UserName, out ConnectionSuccesful);

            if (!ConnectionSuccesful)
            {
                ViewBag.Message = "An error has occured while trying to connect. Please try again later.";
                return View(LoginUser);
            }
            if(user == null)
            {
                ViewBag.Message = "No such account exists.";
                return View();
            }
            if (!user.Password.Equals(LoginUser.Password))
            {
                ViewBag.Message = "Invalid Password. Please try again.";
                return View();
            }
            if (!user.UserName.Equals(LoginUser.UserName))
            {
                ViewBag.Message = "Invalid User Name. Please try again.";
                return View();
            }
            if (!user.RightsApproved)
            {
                ViewBag.Message = "Admin has not yet approved your account rights. Please try again later.";
                return View();
            }

            var ticket = new FormsAuthenticationTicket(version: 1,
                        name: LoginUser.UserName,
                        issueDate: DateTime.Now,
                        expiration: DateTime.Now.AddDays(5). //Expires in 5 days
                        AddSeconds(HttpContext.Session.Timeout),
                        isPersistent: false,
                        userData: user.Role.ToString());

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            HttpContext.Response.Cookies.Add(cookie);
            return RedirectToAction("Index");
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult Fail()
        {
            return View();
        }

        public ActionResult Index()
        {
            if (User.IsInRole("Individual"))
                return RedirectToAction("IndividualStartingPage");

            if (User.IsInRole("Company"))
                return RedirectToAction("CompanyStartingPage");

            if (User.IsInRole("Admin"))
                return RedirectToAction("AdminStartingPage", "User");

            return RedirectToAction("Fail", "User");
        }
        
        [Authorize(Roles = "Admin")]
        public ActionResult AdminStartingPage()
        {
            DbConnection connection = new DbConnection();
            bool ConnectionSuccesful;
            int LoggedInUserId = connection.GetLoggedInUserIdBasedOnUsername(out ConnectionSuccesful);

            if (!ConnectionSuccesful)
                return RedirectToAction("Fail", "User");

            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminManageCompanies()
        {
            List<UserModel> CompaniesList = new List<UserModel>();
            DbConnection connection = new DbConnection();
            bool ConnectionSuccesful;
            CompaniesList.AddRange(connection.AdminCompaniesList(out ConnectionSuccesful));

            if (!ConnectionSuccesful)
                return RedirectToAction("Fail", "User");

            return View(CompaniesList);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminManageIndividuals()
        {
            List<UserModel> IndividualsList = new List<UserModel>();
            DbConnection connection = new DbConnection();
            bool ConnectionSuccesful;
            IndividualsList.AddRange(connection.AdminIndividualsList(out ConnectionSuccesful));

            if (!ConnectionSuccesful)
                return RedirectToAction("Fail", "User");

            ViewBag.UserRole = "Individual";
            return View(IndividualsList);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminGetCompanyProfile(int id = 0)
        {
            DbConnection connection = new DbConnection();
            CompanyProfileModel profile = new CompanyProfileModel();
            bool ConnectionSuccesful;

            if (id < 1)
                return RedirectToAction("Index", "Home");

            profile = connection.CheckIfCompanyProfileExists(id, out ConnectionSuccesful);

            if (!ConnectionSuccesful)
                return RedirectToAction("Index", "Home");

            if (profile == null)
                return RedirectToAction("AdminManageCompanies", "User");

            return View(profile);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminGetIndividualProfile(int id = 0)
        {
            DbConnection connection = new DbConnection();
            IndividualProfileModel profile = new IndividualProfileModel();
            bool ConnectionSuccesful;

            if (id < 1)
                return RedirectToAction("Index", "Home");

            profile = connection.CheckIfIndividualProfileExists(id, out ConnectionSuccesful);

            if (!ConnectionSuccesful)
                return RedirectToAction("Index", "Home");

            if (profile == null)
                return RedirectToAction("AdminManageIndividuals", "User");

            return View(profile);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminChangeUserRights(int id = 0, string role = "Empty")
        {
            DbConnection connection = new DbConnection();
            bool ConnectionSuccesful;

            if ((id < 1) || (role == "Empty"))
                return RedirectToAction("Index", "Home");

            ConnectionSuccesful = connection.ChangeUserRights(id);

            if (!ConnectionSuccesful)
                return RedirectToAction("Fail", "User");

            if (role == "Individual")
                return RedirectToAction("AdminManageIndividuals", "User");

            return RedirectToAction("AdminManageCompanies", "User");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminViewAllJobPossitions()
        {
            DbConnection connection = new DbConnection();
            List<JobViewModel> MyJobPossitions = new List<JobViewModel>();
            bool ConnectionSuccesful;
            int LoggedInUserId = connection.GetLoggedInUserIdBasedOnUsername(out ConnectionSuccesful);

            if (ConnectionSuccesful)
                MyJobPossitions.AddRange(connection.GetAdminJobPossitions(out ConnectionSuccesful));

            if (!ConnectionSuccesful)
                return RedirectToAction("Fail", "User");

            return View(MyJobPossitions);
        }

        [Authorize(Roles = "Individual")]
        public ActionResult IndividualStartingPage()
        {
            DbConnection connection = new DbConnection();
            IndividualProfileModel profile = new IndividualProfileModel();
            bool ConnectionSuccesful;
            int LoggedInUserId = connection.GetLoggedInUserIdBasedOnUsername(out ConnectionSuccesful);

            if (ConnectionSuccesful)
                profile = connection.CheckIfIndividualProfileExists(LoggedInUserId, out ConnectionSuccesful);

            if (!ConnectionSuccesful)
                return RedirectToAction("Fail", "User");

            return View(profile);
        }

        [Authorize(Roles = "Individual")]
        public ActionResult IndividualCreateProfile()
        {
            DbConnection connection = new DbConnection();
            IndividualProfileModel profile = new IndividualProfileModel();
            bool ConnectionSuccesful;
            int LoggedInUserId = connection.GetLoggedInUserIdBasedOnUsername(out ConnectionSuccesful);

            if (ConnectionSuccesful)
                profile = connection.CheckIfIndividualProfileExists(LoggedInUserId, out ConnectionSuccesful);

            if (!ConnectionSuccesful)
                return RedirectToAction("Fail", "User");

            if (profile == null)
            {
                ViewBag.LoggedInUserId = LoggedInUserId;
                return View();
            }
            return RedirectToAction("Index", "User");
        }


        [Authorize(Roles = "Individual")]
        [HttpPost]
        public ActionResult IndividualCreateProfile(IndividualProfileModel profile)
        {
            DbConnection connection = new DbConnection();
            bool ConnectionSuccesful;
            ConnectionSuccesful = connection.CreateIndividualProfile(profile);

            if (!ConnectionSuccesful)
            {
                ModelState.AddModelError("", "An error has occured while trying to connect. Please try again later.");
                return View(profile);
            }
            return RedirectToAction("Index", "User");
        }

        [Authorize(Roles = "Individual")]
        public ActionResult IndividualEditProfile()
        {
            DbConnection connection = new DbConnection();
            IndividualProfileModel profile = new IndividualProfileModel();
            bool ConnectionSuccesful;
            int LoggedInUserId = connection.GetLoggedInUserIdBasedOnUsername(out ConnectionSuccesful);

            if (ConnectionSuccesful)
                profile = connection.CheckIfIndividualProfileExists(LoggedInUserId, out ConnectionSuccesful);

            if ((!ConnectionSuccesful) || (profile == null))
                return RedirectToAction("Fail", "User");

            return View(profile);
        }

        [Authorize(Roles = "Individual")]
        [HttpPost]
        public ActionResult IndividualEditProfile(IndividualProfileModel profile)
        {
            DbConnection connection = new DbConnection();
            bool ConnectionSuccesful = connection.EditIndividualProfile(profile);

            if (!ConnectionSuccesful)
            {
                ModelState.AddModelError("", "An error has occured while trying to connect. Please try again later.");
                return View(profile);
            }
            return RedirectToAction("Index", "User");
        }

        [Authorize(Roles = "Individual")]
        public ActionResult IndividualViewProfile()
        {
            DbConnection connection = new DbConnection();
            IndividualProfileModel profile = new IndividualProfileModel();
            bool ConnectionSuccesful;
            int id = connection.GetLoggedInUserIdBasedOnUsername(out ConnectionSuccesful);

            if (ConnectionSuccesful)
                profile = connection.GetIndividualProfile(id, out ConnectionSuccesful);

            if ((!ConnectionSuccesful) || (profile == null))
                return RedirectToAction("fail", "User");

            return View(profile);
        }

        [Authorize(Roles = "Individual")]
        public ActionResult IndividualViewAllJobPossitions()
        {
            DbConnection connection = new DbConnection();
            List<JobViewModel> MyJobPossitions = new List<JobViewModel>();
            IndividualProfileModel profile = new IndividualProfileModel();
            bool ConnectionSuccesful;
            int LoggedInUserId = connection.GetLoggedInUserIdBasedOnUsername(out ConnectionSuccesful);

            if (ConnectionSuccesful)
                profile = connection.CheckIfIndividualProfileExists(LoggedInUserId, out ConnectionSuccesful);

            if (ConnectionSuccesful && (profile == null))
                return RedirectToAction("Index", "User");

            if (ConnectionSuccesful)
                MyJobPossitions.AddRange(connection.GetAllIndividualJobPossitions(out ConnectionSuccesful));

            if (!ConnectionSuccesful)
                return RedirectToAction("Fail", "User");

            return View(MyJobPossitions);
        }

        [Authorize(Roles = "Individual")]
        public ActionResult IndividualUploadCVSpecificJobPosstion(string title, int id = 0)
        {
            DbConnection connection = new DbConnection();
            CVJobDetails JobDetails = new CVJobDetails();
            UploadCVSpecificJobModel CV = new UploadCVSpecificJobModel();
            bool ConnectionSuccesful = false;
            int LoggedInUserId = -1;

            if (id > 0)
                JobDetails = connection.IndividualCheckIfJobPossitionExists(id, out ConnectionSuccesful);
            
            if (ConnectionSuccesful && (JobDetails.JobId > 0))
                LoggedInUserId = connection.GetLoggedInUserIdBasedOnUsername(out ConnectionSuccesful);

            if ((!ConnectionSuccesful) || (JobDetails.JobId != id))
                return RedirectToAction("Fail", "User");

            CV.CompanyId = JobDetails.CompanyId;
            CV.JobId = JobDetails.JobId;
            CV.UserId = LoggedInUserId;
            CV.DescriptiveFileName = string.Concat("Job ID = ", CV.JobId, ", Job Title = ", title, ", Submitted = ");
            return View(CV);
        }

        [Authorize(Roles = "Individual")]
        [HttpPost]
        public ActionResult IndividualUploadCVSpecificJobPosstion(UploadCVSpecificJobModel obj)
        {
            DbConnection connection = new DbConnection();
            bool ConnectionSuccesful = false;
            int CvExists = -1;           

            if ((obj.UserId <= 0) || (obj.JobId <= 0))
                return RedirectToAction("Fail", "User");

            if (obj.File == null)
                return View(obj);

            if (obj.File.ContentLength <= 0)
            {
                ViewBag.FileStatus = "The file seems corrupted.";
                return View(obj);
            }
            string FileExtension = Path.GetExtension(obj.File.FileName).ToLower();

            if (!FileExtension.Equals(".pdf"))
            {
                ViewBag.FileStatus = "Only PDF file format is supported. Please provide a PDF file.";
                return View(obj);
            }
            CvExists = connection.CheckIfCVHasUploadedJobPool(obj.UserId, obj.JobId, out ConnectionSuccesful);

            if (ConnectionSuccesful)
                ConnectionSuccesful = connection.UploadCVJobsPool(obj, CvExists);

            if (!ConnectionSuccesful)
            {
                ViewBag.FileStatus = "There was an error while trying to connect. Please try again later.";
                return View(obj);
            }
            if(CvExists < 1)
                ViewBag.FileStatus = "File has been uploaded successfully.";
            else
                ViewBag.FileStatus = "File has been overwritten successfully.";
            return View();
        }

        [Authorize(Roles = "Company")]
        public ActionResult CompanyStartingPage()
        {
            DbConnection connection = new DbConnection();
            CompanyProfileModel profile = new CompanyProfileModel();
            bool ConnectionSuccesful;
            int LoggedInUserId = connection.GetLoggedInUserIdBasedOnUsername(out ConnectionSuccesful);

            if (ConnectionSuccesful)
                profile = connection.CheckIfCompanyProfileExists(LoggedInUserId, out ConnectionSuccesful);

            if (!ConnectionSuccesful)
                return RedirectToAction("Fail", "User");

            return View(profile);
        }

        [Authorize(Roles = "Company")]
        public ActionResult CompanyCreateProfile()
        {
            DbConnection connection = new DbConnection();
            CompanyProfileModel profile = new CompanyProfileModel();
            bool ConnectionSuccesful;
            int LoggedInUserId = connection.GetLoggedInUserIdBasedOnUsername(out ConnectionSuccesful);

            if (ConnectionSuccesful)
                profile = connection.CheckIfCompanyProfileExists(LoggedInUserId, out ConnectionSuccesful);

            if (!ConnectionSuccesful)
                return RedirectToAction("Fail", "User");

            if (profile == null)
            {
                ViewBag.LoggedInUserId = LoggedInUserId;
                return View();
            }
            return RedirectToAction("Index", "User");
        }

        [Authorize(Roles = "Company")]
        [HttpPost]
        public ActionResult CompanyCreateProfile(CompanyProfileModel profile)
        {
            DbConnection connection = new DbConnection();
            bool ConnectionSuccesful;
            ConnectionSuccesful = connection.CreateCompanyProfile(profile);

            if (!ConnectionSuccesful)
            {
                ModelState.AddModelError("", "An error has occured while trying to connect. Please try again later.");
                return View(profile);
            }
            return RedirectToAction("Index", "User");
        }

        [Authorize(Roles = "Company")]
        public ActionResult CompanyEditProfile()
        {
            DbConnection connection = new DbConnection();
            CompanyProfileModel profile = new CompanyProfileModel();
            bool ConnectionSuccesful;
            int LoggedInUserId = connection.GetLoggedInUserIdBasedOnUsername(out ConnectionSuccesful);

            if (ConnectionSuccesful)
                profile = connection.CheckIfCompanyProfileExists(LoggedInUserId, out ConnectionSuccesful);

            if ((!ConnectionSuccesful) || (profile == null))
                return RedirectToAction("Fail", "User");

            return View(profile);
        }

        [Authorize(Roles = "Company")]
        [HttpPost]
        public ActionResult CompanyEditProfile(CompanyProfileModel profile)
        {
            DbConnection connection = new DbConnection();
            bool ConnectionSuccesful = connection.EditCompanyProfile(profile);

            if (!ConnectionSuccesful)
            {
                ModelState.AddModelError("", "An error has occured while trying to connect. Please try again later.");
                return View(profile);
            }
            return RedirectToAction("Index", "User");
        }

        [Authorize(Roles = "Company")]
        public ActionResult CompanyViewProfile()
        {
            DbConnection connection = new DbConnection();
            CompanyProfileModel profile = new CompanyProfileModel();
            bool ConnectionSuccesful;
            var id = connection.GetLoggedInUserIdBasedOnUsername(out ConnectionSuccesful);

            if (ConnectionSuccesful)
                profile = connection.GetCompanyProfile(id, out ConnectionSuccesful);

            if ((!ConnectionSuccesful) || (profile == null))
                return RedirectToAction("fail", "User");

            return View(profile);
        }

        [Authorize(Roles = "Company")]
        public ActionResult CompanyCreateJobPossition()
        {
            bool ConnectionSuccesful;
            DbConnection connection = new DbConnection();
            CompanyProfileModel profile = new CompanyProfileModel();
            JobModel job = new JobModel();
            int LoggedInUserId = connection.GetLoggedInUserIdBasedOnUsername(out ConnectionSuccesful);

            if (ConnectionSuccesful)
                profile = connection.CheckIfCompanyProfileExists(LoggedInUserId, out ConnectionSuccesful);

            if (ConnectionSuccesful && (profile == null))
                return RedirectToAction("Index", "User");

            if (ConnectionSuccesful)
                ConnectionSuccesful = FillStaticLists();

            if (!ConnectionSuccesful)
            {
                ModelState.AddModelError("", "An error has occured while trying to connect. Please try again later.");
                return RedirectToAction("Index", "User");
            }
            job.JobCategories = ListJobCategories;
            job.JobRegions = ListJobRegions;
            job.WorkHours = ListWorkHours;
            ViewBag.LoggedInUserId = LoggedInUserId;
            return View(job);
        }

        [Authorize(Roles = "Company")]
        [HttpPost]
        public ActionResult CompanyCreateJobPossition(JobModel job)
        {
            bool ConnectionSuccesful = true;
            DbConnection connection = new DbConnection();
            ConnectionSuccesful = connection.CreateNewJobAdvert(job);

            if (!ConnectionSuccesful)
            {
                ModelState.AddModelError("", "An error has occured while trying to connect. Please try again later.");
                job.JobCategories = ListJobCategories;
                job.JobRegions = ListJobRegions;
                job.WorkHours = ListWorkHours;
                return View(job);
            }
            return RedirectToAction("Index", "User");
        }

        [Authorize(Roles = "Company")]
        public ActionResult CompanyEditJobPossition(int id = 0)
        {
            bool ConnectionSuccesful = false;
            int LoggedInUserId = -1;
            DbConnection connection = new DbConnection();
            JobModel job = new JobModel();

            ConnectionSuccesful = FillStaticLists();

            if (ConnectionSuccesful)
                LoggedInUserId = connection.GetLoggedInUserIdBasedOnUsername(out ConnectionSuccesful);

            if (ConnectionSuccesful)
                job = connection.GetCompanySpecificJobPossition(id, out ConnectionSuccesful);

            if ((!ConnectionSuccesful) || (job == null) || (LoggedInUserId != job.UserId))
                return RedirectToAction("Fail", "User");

            if (!job.Active)
                return RedirectToAction("CompanyViewCreatedJobPossitions", "User");

            job.JobCategories = ListJobCategories;
            job.JobRegions = ListJobRegions;
            job.WorkHours = ListWorkHours;
            return View(job);
        }

        [Authorize(Roles = "Company")]
        [HttpPost]
        public ActionResult CompanyEditJobPossition(JobModel job)
        {
            bool ConnectionSuccesful = false;
            DbConnection connection = new DbConnection();

            ConnectionSuccesful = connection.EditJobPossition(job);

            if (!ConnectionSuccesful)
            {
                job.JobCategories = ListJobCategories;
                job.JobRegions = ListJobRegions;
                job.WorkHours = ListWorkHours;
                ModelState.AddModelError("", "An error has occured while trying to connect. Please try again later.");
                return View(job);
            }
            return RedirectToAction("CompanyViewCreatedJobPossitions", "User");
        }

        [Authorize(Roles = "Company")]
        public ActionResult CompanyDeleteJobPossition(int id = 0)
        {
            DbConnection connection = new DbConnection();
            JobModel SpecificJobPossition = new JobModel();
            bool ConnectionSuccesful;

            if(id == 0)
                return RedirectToAction("Index", "User");

            int LoggedInUserId = connection.GetLoggedInUserIdBasedOnUsername(out ConnectionSuccesful);

            if (ConnectionSuccesful)
                SpecificJobPossition = connection.GetCompanySpecificJobPossition(id, out ConnectionSuccesful);

            if (!ConnectionSuccesful)
                return RedirectToAction("Fail", "User");

            if (SpecificJobPossition == null)
                return RedirectToAction("Fail", "User");

            if (LoggedInUserId != SpecificJobPossition.UserId)
                return RedirectToAction("CompanyViewCreatedJobPossitions");           

            if (!SpecificJobPossition.Active)
                return RedirectToAction("CompanyViewCreatedJobPossitions");

            return View(SpecificJobPossition);
        }

        [Authorize(Roles = "Company")]
        [HttpPost]
        public ActionResult CompanyDeleteJobPossition(JobModel job)
        {
            DbConnection connection = new DbConnection();
            bool ConnectionSuccesful = connection.DeleteJobPossition(job.Id);

            if (!ConnectionSuccesful)
                return RedirectToAction("Fail", "User");

            return RedirectToAction("CompanyViewCreatedJobPossitions");
        }

        [Authorize(Roles = "Company")]
        public ActionResult CompanyViewCreatedJobPossitions()
        {
            DbConnection connection = new DbConnection();
            List<JobViewModel> MyJobPossitions = new List<JobViewModel>();
            CompanyProfileModel profile = new CompanyProfileModel();
            bool ConnectionSuccesful;
            int LoggedInUserId = connection.GetLoggedInUserIdBasedOnUsername(out ConnectionSuccesful);

            if (ConnectionSuccesful)
                profile = connection.CheckIfCompanyProfileExists(LoggedInUserId, out ConnectionSuccesful);

            if (ConnectionSuccesful && (profile == null))
                return RedirectToAction("Index", "User");

            if (ConnectionSuccesful)
                MyJobPossitions.AddRange(connection.GetCompanyJobPossitions(LoggedInUserId, out ConnectionSuccesful));

            if (!ConnectionSuccesful)
                return RedirectToAction("Fail", "User");

            return View(MyJobPossitions);
        }

        [Authorize(Roles = "Company")]
        public ActionResult CompanyViewCVs()
        {
            DbConnection connection = new DbConnection();
            List<ViewCVsModel> CVList = new List<ViewCVsModel>();
            CompanyProfileModel profile = new CompanyProfileModel();
            bool ConnectionSuccesful;
            int LoggedInUserId = connection.GetLoggedInUserIdBasedOnUsername(out ConnectionSuccesful);

            if (ConnectionSuccesful)
                profile = connection.CheckIfCompanyProfileExists(LoggedInUserId, out ConnectionSuccesful);

            if (ConnectionSuccesful && (profile == null))
                return RedirectToAction("Index", "User");

            if (ConnectionSuccesful)
                CVList.AddRange(connection.CompanyViewAllCVs(LoggedInUserId, out ConnectionSuccesful));

            if (!ConnectionSuccesful)
                return RedirectToAction("Fail", "User");

            ViewBag.ListIsEmpty = CVList.Count.Equals(0);                         
            return View(CVList);
        }

        [Authorize(Roles = "Company")]
        [HttpGet]
        public ActionResult DownloadCV(int JobId = 0, int IndividualId = 0)
        {
            DbConnection connection = new DbConnection();
            DownloadCVModel file = new DownloadCVModel();
            bool ConnectionSuccesful = false;
            int LoggedInUserId = -1;

            if ((JobId <= 0) || (IndividualId <= 0))
                return RedirectToAction("Index", "User");

            var JobPossition = connection.GetCompanySpecificJobPossition(JobId, out ConnectionSuccesful);

            if(ConnectionSuccesful && (JobPossition == null))
                return RedirectToAction("Index", "User");

            if (ConnectionSuccesful)
                LoggedInUserId = connection.GetLoggedInUserIdBasedOnUsername(out ConnectionSuccesful);

            if (ConnectionSuccesful && (LoggedInUserId != JobPossition.UserId))
                return RedirectToAction("Index", "User");

            if (ConnectionSuccesful)
                file = connection.DownLoadCVBasedOnId(JobId, IndividualId, out ConnectionSuccesful);

            if (!ConnectionSuccesful)
                return RedirectToAction("Fail", "User");

            if (file == null)
                return RedirectToAction("Index", "User");

            return File(file.FileContent, "application/pdf", file.DescriptiveFilename);            
        }

        [Authorize(Roles = "Company")]
        [HttpGet]
        public ActionResult CompanyDownloadCVZip()
        {
            DbConnection connection = new DbConnection();
            List<DownloadCVModel> Files = new List<DownloadCVModel>();
            bool ConnectionSuccesful = false;
            int LoggedInUserId = connection.GetLoggedInUserIdBasedOnUsername(out ConnectionSuccesful);

            if (ConnectionSuccesful)
                Files.AddRange(connection.DownLoadCVZip(LoggedInUserId, out ConnectionSuccesful));

            if ((!ConnectionSuccesful) || Files.Count.Equals(0))
                return RedirectToAction("Fail", "User");

            using (MemoryStream ms = new MemoryStream())
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    foreach(var CV in Files)
                    {
                        var zipArchiveEntry = archive.CreateEntry(CV.DescriptiveFilename, CompressionLevel.Fastest);
                        using (var zipStream = zipArchiveEntry.Open()) zipStream.Write(CV.FileContent, 0, CV.FileContent.Length);
                    }                                        
                }
                return File(ms.ToArray(), "application/zip", "CVArchive.zip");
            }
        }
    }          
}
