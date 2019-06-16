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


namespace Jobz
{
    public class DbConnection
    {       
        static string ConnectionDB = "Jobz.Properties.Settings.Setting";


        public List<JobListModel> LoadJobDropDownTablesIntoList(string choice, out bool ConnectionSuccesful)
        {
            string Querry = GetQuerry(choice);
            List<JobListModel> JobList = new List<JobListModel>();

            try
            {
                using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {
                    JobList.AddRange(db.Query<JobListModel>(Querry));
                    ConnectionSuccesful = true;
                }
            }
            catch
            {
                ConnectionSuccesful = false;
            }
            return JobList;
        }


        public string GetQuerry(string choice)
        {
            if (choice.Equals("Category"))
                return "SELECT * FROM Category";

            else if (choice.Equals("Region"))
                return "SELECT * FROM Regions";

            else
                return "SELECT * FROM WorkHours";
        }


        public int GetLoggedInUserIdBasedOnUsername(out bool ConnectionSuccesful)
        {
            string LoggedInUserName = HttpContext.Current.User.Identity.Name.ToString();
            int LoggedInUserId = 0;

            try
            {
                using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {
                    LoggedInUserId = db.ExecuteScalar<int>("GetLoggedInUserIdBasedOnUsername", new
                    { _UserName = LoggedInUserName }, commandType: CommandType.StoredProcedure);
                }
            }
            catch
            {
                ConnectionSuccesful = false;
                return LoggedInUserId;
            }
            ConnectionSuccesful = true;
            return LoggedInUserId;
        }


        public UserModel LogInUser(string LogInUserName, out bool ConnectionSuccesful)
        {
            UserModel user = new UserModel();

            try
            {
                using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {
                    user = db.Query<UserModel>("LogInUser", new
                    { _UserName = LogInUserName }, 
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
                    ConnectionSuccesful = true;                    
                }
            }
            catch
            {
                ConnectionSuccesful = false;
                return user;
            }
            return user;
        }


       public bool CreateNewAccount(UserModel user, out bool UserNameIsLegit)
       {
           bool ConnectionSuccesful;  
            
           try
           {
               using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
               {
                    db.Execute("CreateNewAccount", new
                    {
                        user.UserName, user.Password, user.Email, Role = user.Role.ToString(),
                        RightsApproved = false                                                                                                             
                    }, commandType: CommandType.StoredProcedure);
               }
            }
            catch(SqlException Ex)
            {
               if (Ex.Number == 2627)
                {
                    ConnectionSuccesful = true;
                    UserNameIsLegit = false;
                    return ConnectionSuccesful;
                }
                ConnectionSuccesful = false;
                UserNameIsLegit = false;
                return ConnectionSuccesful;
            }
            ConnectionSuccesful = true;
            UserNameIsLegit = true;
            return ConnectionSuccesful;
        }


       public UserModel UserEditAccountGet(int id, out bool ConnectionSuccesful)
       {
            UserModel user = new UserModel();

            try
            {
                using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {
                    user = db.Query<UserModel>("UserEditAccountGet", new { id },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
                    ConnectionSuccesful = true;
                }
            }
            catch
            {
                ConnectionSuccesful = false;
            }
            return user;
       }


       public bool UserEditAccountPost(UserModel user, out bool UserNameIsLegit)
       {
            try
            {
                using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {
                    db.Execute("UserEditAccountPost", new
                    {
                        _UserName = user.UserName, _Email = user.Email, _Id = user.Id                                               
                    }, commandType: CommandType.StoredProcedure);
                }                             
            }
            catch(SqlException Ex)
            {
                if (Ex.Number == 2627)
                {
                    UserNameIsLegit = false;
                    return true;
                }
                UserNameIsLegit = false;
                return false;
            }
            UserNameIsLegit = true;
            return true;
        }


        public bool ChangeUserRights(int id = 0)
        {            
            try
            {
                using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {

                    bool RightState = db.ExecuteScalar<bool>("GetUserRightState",
                    new { id }, commandType: CommandType.StoredProcedure);

                    if (RightState)
                         RightState = false;
                    else
                         RightState = true;

                    db.Execute("SetUserRightState", new
                    { _RightsApproved = RightState, _Id = id }, commandType: CommandType.StoredProcedure);
                }
            }
            catch (SqlException)
            {
                return false;
            }
            return true;
        }


        public List<UserModel> AdminCompaniesList(out bool ConnectionSuccesful)
        {
            List<UserModel> CompaniesList = new List<UserModel>();           
            try
            {
                using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {
                   CompaniesList.AddRange(db.Query<UserModel>("AdminCompaniesList", 
                   commandType: CommandType.StoredProcedure).ToList());
                }
            }
            catch
            {
                ConnectionSuccesful = false;
                return CompaniesList;
            }
            ConnectionSuccesful = true;
            return CompaniesList;
        }


        public List<UserModel> AdminIndividualsList(out bool ConnectionSuccesful)
        {
            List<UserModel> IndividualsList = new List<UserModel>();
            try
            {
                using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {
                    IndividualsList.AddRange(db.Query<UserModel>("AdminIndividualsList", 
                    commandType: CommandType.StoredProcedure).ToList());
                }
            }
            catch
            {
                ConnectionSuccesful = false;
                return IndividualsList;
            }
            ConnectionSuccesful = true;
            return IndividualsList;
        }


        public CompanyProfileModel CheckIfCompanyProfileExists(int id, out bool ConnectionSuccesful)
        {          
            CompanyProfileModel Profile = new CompanyProfileModel();
            try
            {
                using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {
                    Profile = db.Query<CompanyProfileModel>("CheckIfCompanyProfileExists",
                    new { id }, commandType: CommandType.StoredProcedure).SingleOrDefault();
                }
            }
            catch
            {
                ConnectionSuccesful = false;
                return Profile;
            }
            ConnectionSuccesful = true;
            return Profile;
        }


        public IndividualProfileModel CheckIfIndividualProfileExists(int id, out bool ConnectionSuccesful)
        {
            IndividualProfileModel Profile = new IndividualProfileModel();
            try
            {
                using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {
                    Profile = db.Query<IndividualProfileModel>("CheckIfIndividualProfileExists",
                    new { id }, commandType: CommandType.StoredProcedure).SingleOrDefault();
                }
            }
            catch
            {
                ConnectionSuccesful = false;
                return Profile;
            }
            ConnectionSuccesful = true;
            return Profile;
        }


        public bool CreateCompanyProfile(CompanyProfileModel profile)
        {
             try
             {
                using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {             
                    db.Execute("CreateCompanyProfile", new
                    {
                        profile.UserId, CompanyName = HttpContext.Current.User.Identity.Name.ToString(),  
                        profile.Description, profile.Adress, profile.AdressNumber, profile.PostalCorridor, profile.OfficialSite                       
                    }, commandType: CommandType.StoredProcedure);
                }
             }
             catch
             {
                return false;
             }
             return true;
        }


        public bool CreateIndividualProfile(IndividualProfileModel profile)
        {
            try
            {
                using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {
                    db.Execute("CreateIndividualProfile", new
                    {
                        profile.UserId, profile.FirstName, profile.LastName,                       
                    }, commandType: CommandType.StoredProcedure);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }


        public IndividualProfileModel GetIndividualProfile(int Id, out bool ConnectionSuccesful)
        {
            IndividualProfileModel profile = new IndividualProfileModel();

            try
            {
                using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {
                    profile = db.Query<IndividualProfileModel>("GetIndividualProfile", new { _Id = Id },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
                }
            }
            catch
            {
                ConnectionSuccesful = false;
                return profile;
            }
            ConnectionSuccesful = true;
            return profile;
        }


        public CompanyProfileModel GetCompanyProfile(int Id, out bool ConnectionSuccesful)
        {
            CompanyProfileModel profile = new CompanyProfileModel();

            try
            {
                using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {
                    profile = db.Query<CompanyProfileModel>("GetCompanyProfile", new { _Id = Id },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
                }
            }
            catch
            {
                ConnectionSuccesful = false;
                return profile;
            }
            ConnectionSuccesful = true;
            return profile;
        }


        public bool EditCompanyProfile(CompanyProfileModel profile)
        {
            try
            {
                using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {                 
                    db.Execute("EditCompanyProfile", new
                    {       
                        _Description = profile.Description, _Adress = profile.Adress,
                        _AdressNumber = profile.AdressNumber, _PostalCorridor = profile.PostalCorridor,
                        _OfficialSite = profile.OfficialSite, _Id = profile.Id                        
                    }, commandType: CommandType.StoredProcedure);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }


        public bool EditIndividualProfile(IndividualProfileModel profile)
        {
            try
            {
                using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {
                    db.Execute("EditIndividualProfile", new
                    {
                       _FirstName = profile.FirstName, _LastName = profile.LastName, _Id = profile.Id                                                
                    }, commandType: CommandType.StoredProcedure);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }


        public bool CreateNewJobAdvert(JobModel job)
        {
            DateTime myDateTime = DateTime.Now;

            try
            {
                using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {                                                         
                    db.Execute("CreateNewJobAdvert", new
                    {
                        job.UserId, job.Title, job.CategoryId, job.RegionId, job.WorkHoursId,
                        job.Openings, job.Content, Active = true                                                                                            
                    }, commandType: CommandType.StoredProcedure);                    
                }                
            }
            catch
            {
                return false;
            }
            return true;
        }


        public bool EditJobPossition(JobModel job)
        {
            try
            {
                using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {
                    db.Execute("EditJobPossition", new
                    {
                        _Title = job.Title, _CategoryId = job.CategoryId, _RegionId = job.RegionId,                       
                        _WorkHoursId = job.WorkHoursId, _Openings = job.Openings, _Content = job.Content,                       
                        id = job.Id
                    }, commandType: CommandType.StoredProcedure);
                }
            }  
            catch
            {
                return false;
            }
            return true;
        }


        public bool DeleteJobPossition(int id)
        {
            try
            {
                using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {
                    db.Execute("DeleteJobPossition", new { Active = false, id }, commandType: CommandType.StoredProcedure);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }


        public List<JobViewModel> GetAdminJobPossitions(out bool ConnectionSuccesful)
        {
            List<JobViewModel> JobPossitions = new List<JobViewModel>();

            try
            {
                using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {
                    JobPossitions.AddRange(db.Query<JobViewModel>("GetAdminJobPossitions", 
                    commandType: CommandType.StoredProcedure).ToList());
                    ConnectionSuccesful = true;
                }
            }
            catch
            {
                ConnectionSuccesful = false;
                return JobPossitions;
            }
            return JobPossitions;
        }


        public List<JobViewModel> GetCompanyJobPossitions(int Id, out bool ConnectionSuccesful)
        {
            List<JobViewModel> JobPossitions = new List<JobViewModel>();

            try
            {
                using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {
                  JobPossitions.AddRange(db.Query<JobViewModel>("GetCompanyJobPossitions", new { Id }, 
                  commandType: CommandType.StoredProcedure).ToList());
                  ConnectionSuccesful = true;
                }
            }
            catch
            {
               ConnectionSuccesful = false;
               return JobPossitions;
            }
            return JobPossitions;
        }


        public List<JobViewModel> GetAllIndividualJobPossitions(out bool ConnectionSuccesful)
        {
            List<JobViewModel> JobPossitions = new List<JobViewModel>();

            try
            {
                using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {                    
                    JobPossitions.AddRange(db.Query<JobViewModel>("GetAllIndividualJobPossitions", new { Flag = true }, 
                    commandType: CommandType.StoredProcedure).ToList());
                    ConnectionSuccesful = true;
                }
            }
            catch
            {
                ConnectionSuccesful = false;
                return JobPossitions;
            }
            return JobPossitions;
        }


        public JobModel GetCompanySpecificJobPossition(int Id, out bool ConnectionSuccesful)
        {
            JobModel JobSpecificPossition = new JobModel();

            try
            { 
                using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {

                    JobSpecificPossition = db.Query<JobModel>("GetCompanySpecificJobPossition", new { Id }, 
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
                    ConnectionSuccesful = true;
                }
            }
            catch
            {
                ConnectionSuccesful = false;
                return JobSpecificPossition;
            }
            return JobSpecificPossition;
        }

        public int CheckIfCVHasUploadedJobPool(int UserId, int JobId, out bool ConnectionSuccesful)
        {
            int CvExists = -1;

            try
            {
                using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {

                    CvExists = db.Query<int>("CheckIfCVExistsInJobsPool", new { UserId, JobId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
                    ConnectionSuccesful = true;
                }
            }
            catch
            {
                ConnectionSuccesful = false;
                return CvExists;
            }
            return CvExists;
        }

        public bool UploadCVJobsPool(UploadCVSpecificJobModel CVModel, int CVExists)
        {
            bool ConnectionSuccesful = false;
            Stream str = CVModel.File.InputStream;
            BinaryReader Br = new BinaryReader(str);
            byte[] FileDet = Br.ReadBytes((int)str.Length);
            str.Close();

            try
            {
                using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {
                    if(CVExists < 1)
                    {
                       db.Execute("UploadCVJobsPool",
                       new { CVModel.CompanyId, UserID = CVModel.UserId, JobID = CVModel.JobId, Filename = CVModel.File.FileName,
                       CVModel.DescriptiveFileName, FileContent = FileDet },
                       commandType: CommandType.StoredProcedure);
                    }
                    else
                    {
                       db.Execute("UpdateIndividualCVPool",
                       new { Id = CVExists, Filename = CVModel.File.FileName, FileContent = FileDet },
                       commandType: CommandType.StoredProcedure);
                    }                   
                }
            }
            catch
            {               
                ConnectionSuccesful = false;
                return ConnectionSuccesful;                
            }
            ConnectionSuccesful = true;
            return ConnectionSuccesful;
        }

        public DownloadCVModel DownLoadCVBasedOnId(int JobId, int IndividualId, out bool ConnectionSuccesful)
        {
            DownloadCVModel file = new DownloadCVModel();
            try
            {
                using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {
                    file = db.Query<DownloadCVModel>("DownLoadCVBasedOnId",
                    new { JobId, IndividualId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
                    ConnectionSuccesful = true;                    
                }
            }
            catch
            {
                ConnectionSuccesful = false;
            }
            return file;
        }


        public List<DownloadCVModel> DownLoadCVZip(int CompanyId, out bool ConnectionSuccesful)
        {
            List<DownloadCVModel> files = new List<DownloadCVModel>();
            try
            {
                using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {
                    files.AddRange(db.Query<DownloadCVModel>("DownLoadCVZip", new { CompanyId },
                    commandType: CommandType.StoredProcedure).ToList());
                    ConnectionSuccesful = true;
                }
            }
            catch
            {
                ConnectionSuccesful = false;
            }
            return files;
        }



        public CVJobDetails IndividualCheckIfJobPossitionExists(int id, out bool ConnectionSuccesful)
        {
            CVJobDetails JobDetails = new CVJobDetails();
            try
            {
                using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {

                    JobDetails = db.Query<CVJobDetails>("IndividualCheckIfJobPossitionExists", new { id },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
                    ConnectionSuccesful = true;
                }
            }
            catch
            {
                ConnectionSuccesful = false;
                return JobDetails;
            }
            return JobDetails;
        }

        public int CompanyCheckIfJobPossitionExists(int id, out bool ConnectionSuccesful)
        {
            try
            {
                using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {

                    id = db.Query<int>("CompanyCheckIfJobPossitionExists", new { id },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
                    ConnectionSuccesful = true;
                }
            }
            catch
            {
                ConnectionSuccesful = false;
                return id;
            }
            return id;
        }

        public List<ViewCVsModel> CompanyViewAllCVs(int LoggedInUserId, out bool ConnectionSuccesful)
        {
            List<ViewCVsModel> CVList = new List<ViewCVsModel>();
            try
            {
                using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionDB].ConnectionString))
                {

                    CVList.AddRange(db.Query<ViewCVsModel>("CompanyViewAllCVs", new { CompanyId = LoggedInUserId },
                    commandType: CommandType.StoredProcedure).ToList());
                    ConnectionSuccesful = true;
                }
            }
            catch
            {
                ConnectionSuccesful = false;
                return CVList;
            }
            return CVList;
        }
    }  
}