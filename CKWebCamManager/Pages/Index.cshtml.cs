using Microsoft.AspNetCore.Mvc.RazorPages;
using CKWebCamManager.Services;
using CKWebCamManager.Models;
using System.Collections.Generic;

namespace CKWebCamManager.Pages
{
    public class IndexModel : PageModel
    {
        private readonly CameraService _cameraService;

        public IndexModel(CameraService cameraService)
        {
            _cameraService = cameraService;
        }

        public List<Camera> Cameras { get; set; }

        public void OnGet()
        {
            Cameras = _cameraService.GetAll();
        }
    }
}