using Microsoft.AspNetCore.Mvc;
using HospitalApp.Models;
using Microsoft.Extensions.Caching.Memory;

namespace HospitalApp.Controllers
{
        public class DoctorController : Controller
    {
        private readonly IMemoryCache _cache;

        public DoctorController(IMemoryCache cache)
        {
            _cache = cache;
        }

        private List<Doctor> GetDoctors()
        {
            return _cache.GetOrCreate("DoctorList", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3);
                Console.WriteLine("Doctor list loaded from source (not from cache)");

                return new List<Doctor>
                {
                    new Doctor { DoctorId = 1, DoctorName = "Dr. Amit Shah", Specialization = "Cardiology", Experience = 12, ConsultationFee = 800 },
                    new Doctor { DoctorId = 2, DoctorName = "Dr. Neha Patel", Specialization = "Dermatology", Experience = 8, ConsultationFee = 600 },
                    new Doctor { DoctorId = 3, DoctorName = "Dr. Rahul Mehta", Specialization = "Orthopedics", Experience = 15, ConsultationFee = 900 },
                    new Doctor { DoctorId = 4, DoctorName = "Dr. Priya Desai", Specialization = "Pediatrics", Experience = 6, ConsultationFee = 500 }
                };
            })!;
        }

        public IActionResult Index()
        {
            var doctors = GetDoctors();

            // Session: read the selected specialization
            var specialization = HttpContext.Session.GetString("Specialization");
            if (!string.IsNullOrEmpty(specialization))
            {
                doctors = doctors.Where(d => d.Specialization == specialization).ToList();
            }

            // Cookie: read the patient name
            ViewBag.PatientName = Request.Cookies["PatientName"];
            ViewBag.SelectedSpecialization = specialization;
            ViewBag.Specializations = GetDoctors().Select(d => d.Specialization).Distinct().ToList();

            return View(doctors);
        }

        [HttpPost]
        public IActionResult SetPatientName(string patientName)
        {
            if (!string.IsNullOrWhiteSpace(patientName))
            {
                // Cookie: store the patient name for 1 day
                var options = new CookieOptions { Expires = DateTimeOffset.Now.AddDays(1) };
                Response.Cookies.Append("PatientName", patientName, options);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult SelectSpecialization(string specialization)
        {
            if (string.IsNullOrEmpty(specialization))
            {
                HttpContext.Session.Remove("Specialization");
            }
            else
            {
                // Session: store the selected specialization
                HttpContext.Session.SetString("Specialization", specialization);
            }
            return RedirectToAction("Index");
        }

                // Query String: DoctorId arrives from the URL, e.g. /Doctor/Details?doctorId=2
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "doctorId" })]
        public IActionResult Details(int doctorId)
        {
            var doctor = GetDoctors().FirstOrDefault(d => d.DoctorId == doctorId);
            if (doctor == null)
            {
                return NotFound();
            }

            ViewBag.PatientName = Request.Cookies["PatientName"];
            return View(doctor);
        }

        // Hidden Field: doctorId arrives from the hidden input in the form
        [HttpPost]
        public IActionResult BookAppointment(int doctorId, string patientName, DateTime appointmentDate)
        {
            var doctor = GetDoctors().FirstOrDefault(d => d.DoctorId == doctorId);
            if (doctor == null)
            {
                return NotFound();
            }

            ViewBag.PatientName = patientName;
            ViewBag.AppointmentDate = appointmentDate;
            return View("Confirmation", doctor);
        }
    }
}