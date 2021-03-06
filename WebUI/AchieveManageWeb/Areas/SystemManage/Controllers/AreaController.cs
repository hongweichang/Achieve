﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AchieveManageWeb.App_Start;
using AchieveCommon;
using AchieveBLL;
using AchieveManageWeb.App_Start.BaseController;
using AchieveEntity;
using System;
using AchieveCommon.Web.Tree;
using AchieveCommon.Web.TreeGrid;
using AchieveCommon.Operator;

namespace AchieveManageWeb.Areas.SystemManage.Controllers
{
    public class AreaController : BaseController
    {
        private Sys_AreaBLL areaApp = new Sys_AreaBLL();

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTreeSelectJson()
        {
            var data = areaApp.GetList();
            var treeList = new List<TreeSelectModel>();
            foreach (Sys_Area item in data)
            {
                TreeSelectModel treeModel = new TreeSelectModel();
                treeModel.id = item.F_Id;
                treeModel.text = item.F_FullName;
                treeModel.parentId = item.F_ParentId;
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeSelectJson());
        }
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTreeGridJson(string keyword)
        {
            var data = areaApp.GetList();
            var treeList = new List<TreeGridModel>();

            if (!string.IsNullOrEmpty(keyword))
            {
                data = Seach(data, keyword);
            }
            foreach (Sys_Area item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.F_ParentId == item.F_Id) == 0 ? false : true;
                treeModel.id = item.F_Id;
                treeModel.text = item.F_FullName;
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.F_ParentId;
                treeModel.expanded = true;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());
        }
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = areaApp.GetForm(keyValue);
            return Content(data.ToJson());
        }
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(Sys_Area areaEntity, string keyValue)
        {
            try
            {
                bool i =false;
                if (keyValue == "" || keyValue == null)
                {
                    areaEntity.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserCode;
                    areaEntity.F_CreatorTime = DateTime.Now;
                    areaEntity.F_Id = System.Guid.NewGuid().ToString();
                    string[] notstr = { "ChildNodes" };
                    i = areaApp.Add(areaEntity, notstr);
                }
                else
                {
                    areaEntity.F_Id = keyValue;
                    areaEntity.F_LastModifyUserId = OperatorProvider.Provider.GetCurrent().UserCode;
                    areaEntity.F_LastModifyTime = DateTime.Now;
                    string[] notstr = { "ChildNodes", "F_CreatorUserId", "F_CreatorTime" };
                    i = areaApp.Update(areaEntity, notstr);
                }
                if (i)
                {
                    return Success("操作成功。");
                }
                else
                {
                    return Warning("操作失败。");
                }

            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            try
            {
                bool i = false;
                i = areaApp.Delete(keyValue);
                if (i)
                {
                    return Success("操作成功。");
                }
                else
                {
                    return Warning("操作失败。");
                }

            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
        [NonAction]
        public List<Sys_Area> Seach(List<Sys_Area> list, string keyValue)
        {
            List<Sys_Area> treeList = new List<Sys_Area>();
            foreach (Sys_Area entity in list.Where(c => c.F_FullName.Contains(keyValue)))
            {
                treeList.Add(entity);
                string pId = entity.F_ParentId;
                while (true)
                {
                    if (string.IsNullOrEmpty(pId) && pId == "0")
                    {
                        break;
                    }
                    var upRecord = list.Where(c => c.F_Id == pId).ToList();
                    if (upRecord != null && upRecord.Count > 0)
                    {
                        treeList.Add(upRecord[0]);
                        pId = upRecord[0].F_ParentId;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return treeList;
        }
    }
}
