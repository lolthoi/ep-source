using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.Project;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Test.Api
{
    [TestClass]
    public class ProjectTest : TestBase
    {
        private readonly Random _rand = new Random();
        const string _url = "/project";
        public List<ProjectModel> _dataInit = new List<ProjectModel>();
        private ProjectModel _projectModel = new ProjectModel();

        [TestInitialize]
        public void InitData()
        {
            InitProjectData();
        }

        [TestCleanup]
        public void CleanData()
        {
            Clear();
        }

        #region GET_ALL

        [TestMethod]
        public void AdminRoleGetAll_ValidDataWithEmptySearchText_Success()
        {
            var result = Helper.AdminGet<List<ProjectModel>>(_url);
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.Data.Count > 0);
        }

        [TestMethod]
        public void AdminRoleGetAll_ValidDataWithSearchText_Succcess() 
        {
            var searchText = "";
            var item = _dataInit.FirstOrDefault();
            if (item != null)
                searchText = item.Name;
            var url = $"{_url}?key ={searchText}";
            var result = Helper.AdminGet<List<ProjectModel>>(url);

            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.Data.Count > 0);
        }

        [TestMethod]
        public void UserRoleGetAll_ValidDataWithEmptySearchText_Success()
        {
            var result = Helper.UserGet<List<ProjectModel>>(_url);
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.Data.Count > 0);
        }

        #endregion

        #region GET_BY_ID

        [TestMethod]
        public void AdminRoleGetById_ValidData_Success()
        {
            int id = 0;
            var item = _dataInit.FirstOrDefault();
            if (item != null)
            {
                id = item.Id;
            }
            var result = Helper.AdminGet<ProjectModel>($"{_url}/{id}");

            Assert.IsNotNull(result.Data);
            Assert.IsNull(result.Error);
            Assert.AreEqual(item.Name, result.Data.Name);
            Assert.AreEqual(item.Description, result.Data.Description);
            Assert.AreEqual(item.Status, result.Data.Status);
        }
        [TestMethod]
        public void AdminRoleGetById_InvalidData_Error()
        {
            int id = 0;
            var result = Helper.AdminGet<ProjectModel>($"{_url}/{id}");

            Assert.IsNull(result.Data);
            Assert.IsNotNull(result.Error);
            var errorMess = JsonConvert.DeserializeObject<string>(result.Error.Message);
            Assert.AreEqual("Project not found", errorMess);
        }

        [TestMethod]
        public void UserRoleGetById_InvalidData_Error()
        {
            int id = 0;
            var item = _dataInit.FirstOrDefault();
            if (item != null)
            {
                id = item.Id;
            }
            var result = Helper.UserGet<ProjectModel>($"{_url}/{id}");

            var errorMess = JsonConvert.DeserializeObject<string>(result.Error.Message);

            Assert.IsNull(result.Data);
            Assert.IsNotNull(result.Error);
            Assert.AreEqual("You do not have access to this project", errorMess);
        }

        #endregion

        #region CREATE

        [TestMethod]
        public void UserRoleAddProject_NoRole_Fail()
        {
            ProjectModel noRole = InitProjectModel();
            var actualModel = Helper.UserPost<ProjectModel>(_url, noRole);

            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);

            Assert.IsNotNull(actualModel);
            Assert.IsNull(actualModel.Data);
            Assert.AreEqual("No Role", errorMess);
        }

        [TestMethod]
        public void AdminRoleAddProject_NullModel_Fail()
        {
            ProjectModel nullModel = null;
            var actualModel = Helper.AdminPost<ProjectModel>(_url, nullModel);

            Assert.IsNotNull(actualModel);
            Assert.IsNull(actualModel.Data);
            Assert.IsNull(actualModel.Error.Message);
        }

        [TestMethod]
        public void AdminRoleAddProject_InvalidNullName_Fail()
        {
            ProjectModel invalidNullName = InitProjectModel();
            invalidNullName.Name = "";

            var actualModel = Helper.AdminPost<ProjectModel>(_url, invalidNullName);

            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);

            Assert.IsNotNull(actualModel);
            Assert.IsNull(actualModel.Data);
            Assert.AreEqual("Name is required", errorMess);
        }

        [TestMethod]
        public void AdminRoleAddProject_invalidMaxLengthName_Fail()
        {
            ProjectModel invalidMaxLengthName = InitProjectModel();
            invalidMaxLengthName.Name = AppendSpaceToString("This string greater than 50 character", 60);

            var actualModel = Helper.AdminPost<ProjectModel>(_url, invalidMaxLengthName);

            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);

            Assert.IsNotNull(actualModel);
            Assert.IsNull(actualModel.Data);
            Assert.AreEqual("Max length of Project name is 50", errorMess);
        }

        [TestMethod]
        public void AdminRoleAddProject_InvalidMaxLengthDescription_Fail()
        {
            ProjectModel invalidMaxLengthDescription = InitProjectModel(); 
            invalidMaxLengthDescription.Description = AppendSpaceToString("This string greater than 500 character", 510);

            var actualModel = Helper.AdminPost<ProjectModel>(_url, invalidMaxLengthDescription);

            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);

            Assert.IsNotNull(actualModel);
            Assert.IsNull(actualModel.Data);
            Assert.AreEqual("Max length of Description is 500", errorMess);
        }

        [TestMethod]
        public void AdminRoleAddProject_InvalidNameIsAlreadyExists_Fail()
        {
            ProjectModel invalidNameIsAlreadyExists = InitProjectModel();
            invalidNameIsAlreadyExists.Name = _dataInit.First().Name;

            var actualModel = Helper.AdminPost<ProjectModel>(_url, invalidNameIsAlreadyExists);

            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);

            Assert.IsNotNull(actualModel);
            Assert.IsNull(actualModel.Data);
            Assert.AreEqual("The Project Name already exists", errorMess);
        }

        [TestMethod]
        public void AdminRoleAddProject_Valid_Success()
        {
            ProjectModel projectModel = InitProjectModel();
            var actualModel = Helper.AdminPost<ProjectModel>(_url, projectModel);
            if (actualModel.Error == null)
            {
                _dataInit.Add(actualModel.Data);
            }
            Assert.AreEqual(projectModel.Name, actualModel.Data.Name);
            Assert.AreEqual(projectModel.Description, actualModel.Data.Description);
            Assert.AreEqual(projectModel.Status, actualModel.Data.Status);
        }

        #endregion

        #region UPDATE

        [TestMethod]
        public void UserRoleUpdateProject_NoRole_Fail()
        {
            ProjectModel noRole = _dataInit.First();
            var actualModel = Helper.UserPut<ProjectModel>(_url, noRole);

            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);

            Assert.IsNotNull(actualModel);
            Assert.IsNull(actualModel.Data);
            Assert.AreEqual("No Role", errorMess);
        }

        [TestMethod]
        public void AdminRoleUpdateProject_NullModel_Fail()
        {
            ProjectModel nullModel = null;
            var actualModel = Helper.AdminPut<ProjectModel>(_url, nullModel);

            Assert.IsNotNull(actualModel);
            Assert.IsNull(actualModel.Data);
            Assert.IsNull(actualModel.Error.Message);
        }

        [TestMethod]
        public void AdminRoleUpdateProject_InvalidNullName_Fail()
        {
            ProjectModel invalidNullName = InitProjectModel();
            invalidNullName.Name = "";

            var actualModel = Helper.AdminPut<ProjectModel>(_url, invalidNullName);

            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);

            Assert.IsNotNull(actualModel);
            Assert.IsNull(actualModel.Data);
            Assert.AreEqual("Name is required", errorMess);
        }

        [TestMethod]
        public void AdminRoleUpdateProject_invalidMaxLengthName_Fail()
        {
            ProjectModel invalidMaxLengthName = InitProjectModel();
            invalidMaxLengthName.Name = AppendSpaceToString("This string greater than 50 character", 60);

            var actualModel = Helper.AdminPut<ProjectModel>(_url, invalidMaxLengthName);

            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);

            Assert.IsNotNull(actualModel);
            Assert.IsNull(actualModel.Data);
            Assert.AreEqual("Max length of Project name is 50", errorMess);
        }

        [TestMethod]
        public void AdminRoleUpdateProject_InvalidMaxLengthDescription_Fail()
        {
            ProjectModel invalidMaxLengthDescription = InitProjectModel();
            invalidMaxLengthDescription.Description = AppendSpaceToString("This string greater than 500 character", 510);

            var actualModel = Helper.AdminPut<ProjectModel>(_url, invalidMaxLengthDescription);

            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);

            Assert.IsNotNull(actualModel);
            Assert.IsNull(actualModel.Data);
            Assert.AreEqual("Max length of Description is 500", errorMess);
        }

        [TestMethod]
        public void AdminRoleUpdateProject_InvalidNameIsAlreadyExists_Fail()
        {
            ProjectModel invalidNameIsAlreadyExists = InitProjectModel();
            var createModel = Helper.AdminPost<ProjectModel>(_url, invalidNameIsAlreadyExists);
            if (createModel.Error == null)
            {
                _dataInit.Add(createModel.Data);
            }

            invalidNameIsAlreadyExists.Name = _dataInit.First().Name;

            var actualModel = Helper.AdminPut<ProjectModel>(_url, invalidNameIsAlreadyExists);

            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);

            Assert.IsNotNull(actualModel);
            Assert.IsNull(actualModel.Data);
            Assert.AreEqual("The Project Name already exists", errorMess);
        }

        [TestMethod]
        public void AdminRoleUpdateProject_Valid_Success()
        {
            ProjectModel projectModel = InitProjectModel();
            projectModel.Id = _dataInit.First().Id;
            var actualModel = Helper.AdminPut<ProjectModel>(_url, projectModel);
            if (actualModel.Error == null)
            {
                _dataInit.Add(actualModel.Data);
            }
            Assert.AreEqual(projectModel.Name, actualModel.Data.Name);
            Assert.AreEqual(projectModel.Description, actualModel.Data.Description);
            Assert.AreEqual(projectModel.Status, actualModel.Data.Status);
        }

        #endregion

        #region DELETE

        [TestMethod]
        public void AdminRoleDeleteProject_Valid_Success()
        {
            ProjectModel item = _dataInit.First();
            var actualModel = Helper.AdminDelete<bool>($"{_url}/{item.Id}");

            Assert.IsNotNull(actualModel);
            Assert.IsNotNull(actualModel.Data);
            Assert.IsTrue(actualModel.Data);
            Assert.IsNull(actualModel.Error);       
        }

        [TestMethod]
        public void AdminRoleDeleteProject_InvalidData_Error()
        {
            ProjectModel item = _dataInit.First();
            item.Id = 0;
            var actualModel = Helper.AdminDelete<bool>($"{_url}/{item.Id}");

            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);

            Assert.IsNotNull(actualModel);
            Assert.IsFalse(actualModel.Data);
            Assert.AreEqual("Project not found", errorMess);

        }

        [TestMethod]
        public void UserRoleDeleteProject_NoRole_Fail()
        {
            ProjectModel item = _dataInit.First();
            var actualModel = Helper.UserDelete<bool>($"{_url}/{item.Id}");

            var errorMess = JsonConvert.DeserializeObject<string>(actualModel.Error.Message);

            Assert.IsNotNull(actualModel);
            Assert.IsFalse(actualModel.Data);
            Assert.AreEqual("No Role", errorMess);
        }

        #endregion


        #region Init Project


        public ProjectModel InitProjectModel()
        {
            return new ProjectModel()
            {
                Name = "Name " + rand(),
                Status = Models.Common.ProjectStatusEnum.CLOSED,
                Description = "Create Description " + rand()
            };
        }

        private void InitProjectData()
        {
            var projectModel = InitProjectModel();
            var result = Helper.AdminPost<ProjectModel>(_url, projectModel);

            if (result.Error == null)
            {
                _dataInit.Add(result.Data);
            }
        }
        private void Clear()
        {
            _dataInit.ForEach(x =>
            {
                if (x.Id != 0)
                {
                    var url = _url + "/" + x.Id;
                    Helper.AdminDelete<bool>(url);
                }
            });
        }

        #endregion

        #region Random func
        private int rand()
        {
            return _rand.Next(1, 1000000);
        }
        #endregion

        #region private

        private string AppendSpaceToString(string str, int numberOfLength)
        {
            StringBuilder sb = new StringBuilder();
            if ((numberOfLength - str.Length) > 0)
            {
                sb.Append(' ', (numberOfLength - str.Length));
            }
            sb.Append(str);
            return sb.ToString();
        }
        #endregion
    }
}
