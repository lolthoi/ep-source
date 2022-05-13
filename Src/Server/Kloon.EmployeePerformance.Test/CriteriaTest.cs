using Kloon.EmployeePerformance.Models.Criteria;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kloon.EmployeePerformance.Test
{
    [TestClass]
    public class CriteriaTest : TestBase
    {
        private readonly Random _rand = new Random();
        const string _url = "/Criteria";
        public List<CriteriaModel> dataInit = new List<CriteriaModel>();

        [TestInitialize]
        public void InitData()
        {
            InitCriteriaData();
        }

        [TestCleanup]
        public void CleanData()
        {
            Clear();
        }


        #region Get
        [TestMethod]
        public void Criteria_Admin_GetAll_When_VailidData_Then_Success()
        {
            var result = Helper.AdminGet<List<CriteriaModel>>(_url);
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.Data.Count > 0);
        }

        [TestMethod]
        public void Criteria_Admin_GetAll_Search_When_VailidData_Then_Success()
        {
            var key = "";
            var item = dataInit.FirstOrDefault();
            if (item != null)
                key = item.Name;
            var url = $"{_url}?key={key}";
            var result = Helper.AdminGet<List<CriteriaModel>>(url);
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.Data.Count > 0);
        }

        [TestMethod]
        public void Criteria_User_GetAll_Search_When_VailidData_Then_Error()
        {
            var key = "";
            var item = dataInit.FirstOrDefault();
            if (item != null)
                key = item.Name;
            var url = $"{_url}?key={key}";
            var result = Helper.UserGet<List<CriteriaModel>>(url);
            var errorMess = JsonConvert.DeserializeObject<string>(result.Error.Message);
            Assert.IsNull(result.Data);
            Assert.AreEqual("No Role", errorMess);
        }

        [TestMethod]
        public void Criteria_User_GetAll_When_VailidData_Then_Error()
        {
            var result = Helper.UserGet<List<CriteriaModel>>(_url);
            var errorMess = JsonConvert.DeserializeObject<string>(result.Error.Message);
            Assert.IsNull(result.Data);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("No Role", errorMess);
        }

        [TestMethod]
        public void Criteria_Admin_GetById_When_VailidData_Then_Success()
        {
            Guid id = Guid.Empty;
            var item = dataInit.FirstOrDefault();
            if (item != null)
                id = item.Id;
            var uri = $"{_url}/{id}";
            var result = Helper.AdminGet<CriteriaModel>(uri);
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(item.Name, result.Data.Name);
        }

        [TestMethod]
        public void Criteria_Admin_GetById_When_InVailidData_Then_Return_Error()
        {
            Guid id = Guid.Empty;
            var uri = $"{_url}/{id}";
            var result = Helper.AdminGet<CriteriaModel>(uri);
            Assert.IsNotNull(result.Error);
            Assert.IsFalse(result.IsSuccess);
            var errorMess = JsonConvert.DeserializeObject<string>(result.Error.Message);
            Assert.AreEqual<string>("INVALID_ID", errorMess);
        }

        [TestMethod]
        public void Criteria_User_GetById_When_NoRole_Then_Error()
        {
            Guid id = Guid.Empty;
            var item = dataInit.FirstOrDefault();
            if (item != null)
                id = item.Id;
            var uri = $"{_url}/{id}";
            var result = Helper.UserGet<CriteriaModel>(uri);
            var errorMess = JsonConvert.DeserializeObject<string>(result.Error.Message);
            Assert.IsNotNull(result.Error);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("No Role", errorMess);
        }
        #endregion

        #region Add
        [TestMethod]
        public void Criteria_Admin_Add_When_ValidModel_Then_Sucssess()
        {
            Guid criteriaId = Guid.Empty;
            Guid typeId = Guid.Empty;
            var criteriaType = BuildCriteriaTypeModel();
            var createTypeResult = Helper.AdminPost<CriteriaModel>(_url, criteriaType);

            typeId = createTypeResult.Data.Id;
            dataInit.Add(createTypeResult.Data);
            var criteriaModel = BuildCriteriaModel(createTypeResult.Data.Id);
            var createResult = Helper.AdminPost<CriteriaModel>(_url, criteriaModel);
            if (createResult.Error == null)
            {
                dataInit.Add(createResult.Data);
                criteriaId = createResult.Data.Id;
            }

            var typeResult = Helper.AdminGet<CriteriaModel>($"{_url}/{typeId}");
            var criteriaResult = Helper.AdminGet<CriteriaModel>($"{_url}/{criteriaId}");

            Assert.AreEqual(criteriaType.Name, typeResult.Data.Name);
            Assert.AreEqual(criteriaModel.Name, criteriaResult.Data.Name);
        }

        [TestMethod]
        public void Criteria_Admin_Add_When_InValidModel_Then_Error()
        {
            Guid criteriaId = Guid.Empty;
            Guid typeId = Guid.Empty;
            var criteriaType = BuildCriteriaTypeModel();
            var createTypeResult = Helper.AdminPost<CriteriaModel>(_url, criteriaType);

            typeId = createTypeResult.Data.Id;
            dataInit.Add(createTypeResult.Data);
            var criteriaModel = BuildCriteriaModel(createTypeResult.Data.Id);

            // set Name = string.empty
            criteriaModel.Name = "";

            var createResult = Helper.AdminPost<CriteriaModel>(_url, criteriaModel);
            var errMess = JsonConvert.DeserializeObject<string>(createResult.Error.Message);
            Assert.AreEqual("INVALID_MODEL", errMess);
            Assert.IsFalse(createResult.IsSuccess);
            Assert.IsNotNull(createResult.Error);
        }

        [TestMethod]
        public void Criteria_Admin_Add_When_DuplicateType_Then_Error()
        {
            Guid criteriaId = Guid.Empty;
            Guid typeId = Guid.Empty;
            var criteriaType = BuildCriteriaTypeModel();
            var createTypeResult = Helper.AdminPost<CriteriaModel>(_url, criteriaType);
            dataInit.Add(createTypeResult.Data);

            var typeDuplicate = BuildCriteriaTypeModel();
            // Set Duplicate Name
            typeDuplicate.Name = criteriaType.Name;
            var createTypeDuplicateResult = Helper.AdminPost<CriteriaModel>(_url, typeDuplicate);
            var errMess = JsonConvert.DeserializeObject<string>(createTypeDuplicateResult.Error.Message);

            Assert.AreEqual("CRITERIA_TYPE_DUPLICATE", errMess);
            Assert.IsFalse(createTypeDuplicateResult.IsSuccess);
            Assert.IsNotNull(createTypeDuplicateResult.Error);
        }

        [TestMethod]
        public void Criteria_Admin_Add_When_Duplicate_Criteria_Then_Error()
        {
            Guid criteriaId = Guid.Empty;
            Guid typeId = Guid.Empty;
            var criteriaType = BuildCriteriaTypeModel();
            var createTypeResult = Helper.AdminPost<CriteriaModel>(_url, criteriaType);

            typeId = createTypeResult.Data.Id;
            dataInit.Add(createTypeResult.Data);
            var criteriaModel = BuildCriteriaModel(createTypeResult.Data.Id);
            var createResult = Helper.AdminPost<CriteriaModel>(_url, criteriaModel);
            dataInit.Add(createResult.Data);

            var duplicateCriteria = BuildCriteriaModel(createTypeResult.Data.Id);
            // Set Duplicate Name
            duplicateCriteria.Name = criteriaModel.Name;

            var createDuplicateResult = Helper.AdminPost<CriteriaModel>(_url, duplicateCriteria);
            var errMess = JsonConvert.DeserializeObject<string>(createDuplicateResult.Error.Message);

            Assert.AreEqual("CRITERIA_DUPLICATE", errMess);
            Assert.IsFalse(createDuplicateResult.IsSuccess);
            Assert.IsNotNull(createDuplicateResult.Error);
        }
        #endregion

        #region Edit
        [TestMethod]
        public void Criteria_Admin_Edit_When_ValidModel_Then_Sucssess()
        {
            var newName = "New Name Edit" + rand();
            var item = dataInit.First();
            var getResult = Helper.AdminGet<CriteriaModel>($"{_url}/{item.Id}");

            var editModel = getResult.Data;
            editModel.Name = newName;

            var editResult = Helper.AdminPut<CriteriaModel>(_url, editModel);

            Assert.IsTrue(editResult.IsSuccess);
            Assert.AreEqual(newName, editResult.Data.Name);
        }

        [TestMethod]
        public void Criteria_Admin_Edit_When_InValidModel_Then_Error()
        {
            var newName = "New Name Edit" + rand();
            var item = dataInit.First();
            var getResult = Helper.AdminGet<CriteriaModel>($"{_url}/{item.Id}");

            var editModel = getResult.Data;
            editModel.Name = "";

            var editResult = Helper.AdminPut<CriteriaModel>(_url, editModel);

            Assert.IsFalse(editResult.IsSuccess);
            var errorMes = JsonConvert.DeserializeObject<string>(editResult.Error.Message);
            Assert.AreEqual("INVALID_MODEL", errorMes);
        }

        [TestMethod]
        public void Criteria_Admin_Edit_When_NotFoundType_Then_Error()
        {
            var newName = "New Name Edit" + rand();
            var item = dataInit.First(x => x.TypeId == null);
            var getResult = Helper.AdminGet<CriteriaModel>($"{_url}/{item.Id}");

            var editModel = getResult.Data;
            // set id = new id
            editModel.Id = Guid.NewGuid();

            var editResult = Helper.AdminPut<CriteriaModel>(_url, editModel);

            Assert.IsFalse(editResult.IsSuccess);
            var errorMes = JsonConvert.DeserializeObject<string>(editResult.Error.Message);
            Assert.AreEqual("NOTFOUND_CRITERIATYPE", errorMes);
        }

        [TestMethod]
        public void Criteria_Admin_Edit_When_NotFound_Then_Error()
        {
            var newName = "New Name Edit" + rand();
            var item = dataInit.First(x => x.TypeId != null);
            var getResult = Helper.AdminGet<CriteriaModel>($"{_url}/{item.Id}");

            var editModel = getResult.Data;
            // set id = new id
            editModel.Id = Guid.NewGuid();

            var editResult = Helper.AdminPut<CriteriaModel>(_url, editModel);

            Assert.IsFalse(editResult.IsSuccess);
            var errorMes = JsonConvert.DeserializeObject<string>(editResult.Error.Message);
            Assert.AreEqual("NOTFOUND_CRITERIA", errorMes);
        }
        #endregion

        #region Delete
        [TestMethod]
        public void Criteria_Admin_Delete_When_ValidModel_Then_Sucssess()
        {
            var newName = "New Name Edit" + rand();
            var item = dataInit.First();
            var getResult = Helper.AdminGet<CriteriaModel>($"{_url}/{item.Id}");
            Assert.IsNotNull(getResult.Data);


            // delete Item 
            var deleteResult = Helper.AdminDelete<bool>($"{_url}/{item.Id}");
            Assert.IsTrue(deleteResult.Data);

            // Get delete Item 
            var itemDelete = Helper.AdminGet<CriteriaModel>($"{_url}/{item.Id}");
            Assert.IsNull(itemDelete.Data);
        }

        [TestMethod]
        public void Criteria_Admin_Delete_When_InValidModel_Then_Error()
        {
            var item = dataInit.First();
            var getResult = Helper.AdminGet<CriteriaModel>($"{_url}/{item.Id}");
            Assert.IsNotNull(getResult.Data);

            // Set Id = new Id
            item.Id = Guid.Empty; 

            // delete Item 
            var deleteResult = Helper.AdminDelete<bool>($"{_url}/{item.Id}");
            var errorMess = JsonConvert.DeserializeObject<string>(deleteResult.Error.Message);
            Assert.IsFalse(deleteResult.IsSuccess);
            Assert.AreEqual("INVALID_ID", errorMess);
        }

        [TestMethod]
        public void Criteria_Admin_Delete_When_NoFound_Then_Error()
        {
            var item = dataInit.First();
            var getResult = Helper.AdminGet<CriteriaModel>($"{_url}/{item.Id}");
            Assert.IsNotNull(getResult.Data);

            // Set Id = new Id
            item.Id = Guid.NewGuid();

            // delete Item 
            var deleteResult = Helper.AdminDelete<bool>($"{_url}/{item.Id}");
            var errorMess = JsonConvert.DeserializeObject<string>(deleteResult.Error.Message);
            Assert.IsFalse(deleteResult.IsSuccess);
            Assert.AreEqual("NOTFOUND", errorMess);
        }
        #endregion

        private CriteriaModel BuildCriteriaModel(Guid typeId)
        {
            var model = new CriteriaModel
            {
                Name = "Criteria Name " + rand(),
                TypeId = typeId,
                Description = "Criteria DesCription " + rand()
            };

            return model;
        }


        private void InitCriteriaData()
        {
            var criteriaType = BuildCriteriaTypeModel();
            var createTypeResult = Helper.AdminPost<CriteriaModel>(_url, criteriaType);

            if (createTypeResult.Error == null)
            {
                dataInit.Add(createTypeResult.Data);
                var criteriaModel = BuildCriteriaModel(createTypeResult.Data.Id);
                var createResult = Helper.AdminPost<CriteriaModel>(_url, criteriaModel);
                if (createResult.Error == null)
                    dataInit.Add(createResult.Data);
            }
        }

        private void Clear()
        {
            dataInit.ForEach(x =>
            {
                if (x.Id != Guid.Empty)
                {
                    var uri = _url + "/" + x.Id;
                    Helper.AdminDelete<bool>(uri);
                }
            });
        }

        private CriteriaModel BuildCriteriaTypeModel()
        {
            var model = new CriteriaModel
            {
                Name = "Criteria Type " + rand(),
                TypeId = null,
                Description = "Criteria Type DesCription " + rand()
            };

            return model;
        }

        private int rand()
        {
            return _rand.Next(1, 100000);
        }

    }
}
