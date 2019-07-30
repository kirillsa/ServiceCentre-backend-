﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using BLL.DTO;
using BLL.Interfaces;
using Microsoft.AspNet.Identity.Owin;
using PrL.Models;

namespace PrL.Controllers
{
    [Authorize]
    public class ApplicationController : ApiController
    {
        private IUserService UserService
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Get<IMainService>().UserServices;
            }
        }

        private IRepositoryBll<ApplicationDTO> ApplicationsService
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Get<IMainService>().ApplicationServices;
            }
        }

        private IRepositoryBll<StatusDTO> StatusService
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Get<IMainService>().StatusServices;
            }
        }

        [Authorize]
        public IHttpActionResult GetAllApplications()
        {
            if (User.IsInRole("admin"))
            {
                return Ok(ApplicationsService.GetAll());
            }
            if (User.IsInRole("manager"))
            {
                var currentUser = UserService.GetAllUsers().Where(x => x.UserName == User.Identity.Name).First();
                var statusOfApplicationToShow = StatusService.Find(y => y.Name == "new").First();
                IEnumerable<ApplicationDTO> appList = ApplicationsService.Find(x => x.StatusId == statusOfApplicationToShow.Id || x.ExecutorId == currentUser.Id);
                return Ok(appList);
            }
            if (User.IsInRole("user"))
            {
                var currentUser = UserService.GetAllUsers().Where(x => x.UserName == User.Identity.Name).First();
                IEnumerable<ApplicationDTO> appList = ApplicationsService.Find(x => x.UserOwnerId == currentUser.Id);
                return Ok(appList);
            }
            return Content(HttpStatusCode.Forbidden, "You have no rights for this content");
        }

        [Authorize]
        public IHttpActionResult Get(string id)
        {
            ApplicationDTO appDTO = ApplicationsService.Get(id);
            if (appDTO == null)
            {
                return NotFound();
            }
            return Ok(appDTO);
        }

        [HttpPost]
        [Authorize(Roles = "admin,user")]
        public IHttpActionResult CreateApplication(ApplicationCreateModel app)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ApplicationDTO newApp = new ApplicationDTO()
                                    {
                                        ApplicationName = app.ApplicationName,
                                        UserOwnerId = app.UserOwnerId
                                    };
            try
            {
                ApplicationsService.Create(newApp);
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
            return CreatedAtRoute("DefaultApi", new { id = newApp.Id }, newApp);
        }

        [HttpPut]
        [Authorize]
        public IHttpActionResult EditApplication(ApplicationEditModel app)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ApplicationDTO editedApp = new ApplicationDTO()
                                        {
                                            Id = app.Id,
                                            ApplicationName = app.ApplicationName,
                                            StatusId = app.StatusId,
                                            ExecutorId = app.ExecutorId
                                        };
            try
            {
                ApplicationsService.Edit(editedApp);
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
            return Ok(editedApp);
        }

        [Authorize(Roles = "admin,user")]
        public IHttpActionResult DeleteApplication(string id)
        {
            try
            {
                ApplicationsService.Delete(id);
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
            return Ok();
        }
    }
}