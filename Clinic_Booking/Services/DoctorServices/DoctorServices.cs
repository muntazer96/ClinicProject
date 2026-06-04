using Clinic_Booking.Data;
using Clinic_Booking.DTOs.DoctorDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.Doctor;
using Clinic_Booking.Extensions;
using Clinic_Booking.IServices.IDoctorServices;
using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.Entities.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.DoctorServices
{
    public class DoctorServices : IDoctorServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoadServices _load;
        private readonly UserManager<AspNetUsers> _userManager;
        public DoctorServices(ApplicationDbContext context, ILoadServices load, UserManager<AspNetUsers> userManager)
        {
            _context = context;
            _load = load;
            _userManager = userManager;
        }

        public async Task<ActionResult<PaginationDto.PageResult<PublicDoctorListDto>>> SearchPublicAsync(
            SearchDoctorDto form, int page = 1, int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "قيم الصفحة أو الحجم غير صحيحة."
                });
            }

            var now = DateTime.UtcNow;
            var query = GetPublicDoctorsQuery(form);
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var doctors = await ApplyPublicSort(query, form.Sort, now)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new PublicDoctorListDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    NormalizedName = d.NormalizedName,
                    SpecializationId = d.SpecializationId,
                    SpecializationName = d.Specialization.Name,
                    SpecializationNormalizedName = d.Specialization.NormalizedName,
                    Description = d.Description,
                    ImageName = d.ImageName,
                    CanBookOnline =
                        d.DoctorSubscriptions.Any(subscription =>
                            subscription.Status == Clinic_Booking.Enums.SubscriptionStatus.Active &&
                            subscription.StartDate <= now &&
                            subscription.EndDate >= now &&
                            subscription.Package.EBooking) &&
                        d.DoctorFeatures.Any(feature =>
                            !feature.IsDeleted &&
                            feature.IsEnabled &&
                            feature.Feature.NormalizedName == "EBooking"),
                    AverageRating =
                        d.DoctorSubscriptions.Any(subscription =>
                            subscription.Status == Clinic_Booking.Enums.SubscriptionStatus.Active &&
                            subscription.StartDate <= now &&
                            subscription.EndDate >= now &&
                            subscription.Package.ShowReviews) &&
                        d.DoctorFeatures.Any(feature =>
                            !feature.IsDeleted &&
                            feature.IsEnabled &&
                            feature.Feature.NormalizedName == "ShowReviews")
                            ? d.Reviews.Where(review => !review.IsDeleted)
                                .Select(review => (double?)review.Rating)
                                .Average()
                            : null,
                    ReviewCount =
                        d.DoctorSubscriptions.Any(subscription =>
                            subscription.Status == Clinic_Booking.Enums.SubscriptionStatus.Active &&
                            subscription.StartDate <= now &&
                            subscription.EndDate >= now &&
                            subscription.Package.ShowReviews) &&
                        d.DoctorFeatures.Any(feature =>
                            !feature.IsDeleted &&
                            feature.IsEnabled &&
                            feature.Feature.NormalizedName == "ShowReviews")
                            ? d.Reviews.Count(review => !review.IsDeleted)
                            : 0,
                    Clinics = d.Clinics
                        .Where(clinic => !clinic.IsDeleted && clinic.IsVisible)
                        .OrderBy(clinic => clinic.Id)
                        .Select(clinic => new PublicDoctorClinicSummaryDto
                        {
                            Id = clinic.Id,
                            Name = clinic.Name,
                            IraqiProvince = clinic.IraqiProvince,
                            IraqiProvinceName = clinic.IraqiProvince.GetDisplayName(),
                            Address = clinic.Address
                        })
                        .ToList()
                })
                .ToListAsync();

            return new OkObjectResult(new ResponseDto<PaginationDto.PageResult<PublicDoctorListDto>>
            {
                Status = "Success",
                Code = 200,
                Message = "تم جلب الأطباء المتاحين للعامة بنجاح.",
                Data = new PaginationDto.PageResult<PublicDoctorListDto>(
                    doctors, totalItems, totalPages, page, pageSize)
            });
        }

        public async Task<IActionResult> GetPublicProfileAsync(int doctorId)
        {
            var now = DateTime.UtcNow;
            var doctor = await GetPublicDoctorsQuery(new SearchDoctorDto { Id = doctorId })
                .Select(d => new PublicDoctorProfileDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    NormalizedName = d.NormalizedName,
                    SpecializationId = d.SpecializationId,
                    SpecializationName = d.Specialization.Name,
                    SpecializationNormalizedName = d.Specialization.NormalizedName,
                    Description = d.Description,
                    ImageName = d.ImageName,
                    CanBookOnline =
                        d.DoctorSubscriptions.Any(subscription =>
                            subscription.Status == Clinic_Booking.Enums.SubscriptionStatus.Active &&
                            subscription.StartDate <= now &&
                            subscription.EndDate >= now &&
                            subscription.Package.EBooking) &&
                        d.DoctorFeatures.Any(feature =>
                            !feature.IsDeleted &&
                            feature.IsEnabled &&
                            feature.Feature.NormalizedName == "EBooking"),
                    AverageRating =
                        d.DoctorSubscriptions.Any(subscription =>
                            subscription.Status == Clinic_Booking.Enums.SubscriptionStatus.Active &&
                            subscription.StartDate <= now &&
                            subscription.EndDate >= now &&
                            subscription.Package.ShowReviews) &&
                        d.DoctorFeatures.Any(feature =>
                            !feature.IsDeleted &&
                            feature.IsEnabled &&
                            feature.Feature.NormalizedName == "ShowReviews")
                            ? d.Reviews.Where(review => !review.IsDeleted)
                                .Select(review => (double?)review.Rating)
                                .Average()
                            : null,
                    ReviewCount =
                        d.DoctorSubscriptions.Any(subscription =>
                            subscription.Status == Clinic_Booking.Enums.SubscriptionStatus.Active &&
                            subscription.StartDate <= now &&
                            subscription.EndDate >= now &&
                            subscription.Package.ShowReviews) &&
                        d.DoctorFeatures.Any(feature =>
                            !feature.IsDeleted &&
                            feature.IsEnabled &&
                            feature.Feature.NormalizedName == "ShowReviews")
                            ? d.Reviews.Count(review => !review.IsDeleted)
                            : 0,
                    Clinics = d.Clinics
                        .Where(clinic => !clinic.IsDeleted && clinic.IsVisible)
                        .OrderBy(clinic => clinic.Id)
                        .Select(clinic => new PublicDoctorClinicDto
                        {
                            Id = clinic.Id,
                            Name = clinic.Name,
                            IraqiProvince = clinic.IraqiProvince,
                            IraqiProvinceName = clinic.IraqiProvince.GetDisplayName(),
                            Address = clinic.Address,
                            Latitude = clinic.Latitude,
                            Longitude = clinic.Longitude,
                            MapUrl = clinic.MapUrl,
                            PhoneNumber = clinic.PhoneNumber,
                            Availabilities = clinic.Availabilities
                                .Where(availability => !availability.IsDeleted && availability.IsAvailable)
                                .OrderBy(availability => availability.DayId)
                                .Select(availability => new PublicClinicAvailabilityDto
                                {
                                    DayId = availability.DayId,
                                    DayName = availability.Day.Name,
                                    DayNormalizedName = availability.Day.NormalizedName,
                                    StartTime = availability.StartTime,
                                    EndTime = availability.EndTime,
                                    MaxAppointments = availability.MaxAppointments
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (doctor == null)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "الطبيب غير موجود أو غير متاح للعرض العام."
                });
            }

            return new OkObjectResult(new ResponseDto<PublicDoctorProfileDto>
            {
                Status = "Success",
                Code = 200,
                Message = "تم جلب صفحة الطبيب بنجاح.",
                Data = doctor
            });
        }

        private IQueryable<Doctor> GetPublicDoctorsQuery(SearchDoctorDto form)
        {
            var query = _context.Doctors
                .AsNoTracking()
                .Where(d =>
                    !d.IsDeleted &&
                    d.IsPubliclyVisible &&
                    d.Clinics.Any(clinic => !clinic.IsDeleted && clinic.IsVisible))
                .Where(d => form.Id == null || d.Id == form.Id)
                .Where(d => form.Specialization == null || d.SpecializationId == form.Specialization)
                .Where(d => form.IraqiProvince == null || d.Clinics.Any(clinic =>
                    !clinic.IsDeleted &&
                    clinic.IsVisible &&
                    clinic.IraqiProvince == form.IraqiProvince));

            if (!string.IsNullOrWhiteSpace(form.Name))
            {
                var name = form.Name.Trim();
                query = query.Where(d => d.Name.Contains(name) || d.NormalizedName.Contains(name));
            }

            return query;
        }

        private static IOrderedQueryable<Doctor> ApplyPublicSort(
            IQueryable<Doctor> query,
            string? sort,
            DateTime now)
        {
            var normalizedSort = sort?.Trim().ToLowerInvariant();
            return normalizedSort switch
            {
                "rating" => query
                    .OrderByDescending(d =>
                        d.DoctorSubscriptions.Any(subscription =>
                            subscription.Status == Clinic_Booking.Enums.SubscriptionStatus.Active &&
                            subscription.StartDate <= now &&
                            subscription.EndDate >= now &&
                            subscription.Package.ShowReviews) &&
                        d.DoctorFeatures.Any(feature =>
                            !feature.IsDeleted &&
                            feature.IsEnabled &&
                            feature.Feature.NormalizedName == "ShowReviews")
                            ? d.Reviews
                                .Where(review => !review.IsDeleted)
                                .Select(review => (double?)review.Rating)
                                .Average() ?? 0
                            : 0)
                    .ThenByDescending(d => d.SubscriptionRank)
                    .ThenBy(d => d.Name),
                "reviews" => query
                    .OrderByDescending(d =>
                        d.DoctorSubscriptions.Any(subscription =>
                            subscription.Status == Clinic_Booking.Enums.SubscriptionStatus.Active &&
                            subscription.StartDate <= now &&
                            subscription.EndDate >= now &&
                            subscription.Package.ShowReviews) &&
                        d.DoctorFeatures.Any(feature =>
                            !feature.IsDeleted &&
                            feature.IsEnabled &&
                            feature.Feature.NormalizedName == "ShowReviews")
                            ? d.Reviews.Count(review => !review.IsDeleted)
                            : 0)
                    .ThenByDescending(d => d.SubscriptionRank)
                    .ThenBy(d => d.Name),
                "booking" => query
                    .OrderByDescending(d =>
                        d.DoctorSubscriptions.Any(subscription =>
                            subscription.Status == Clinic_Booking.Enums.SubscriptionStatus.Active &&
                            subscription.StartDate <= now &&
                            subscription.EndDate >= now &&
                            subscription.Package.EBooking) &&
                        d.DoctorFeatures.Any(feature =>
                            !feature.IsDeleted &&
                            feature.IsEnabled &&
                            feature.Feature.NormalizedName == "EBooking"))
                    .ThenByDescending(d => d.SubscriptionRank)
                    .ThenBy(d => d.Name),
                _ => query
                    .OrderByDescending(d => d.SubscriptionRank)
                    .ThenBy(d => d.Name),
            };
        }

        public async Task<IActionResult> GetMyProfileAsync()
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
            {
                return new UnauthorizedObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 401,
                    Message = "يجب تسجيل الدخول أولاً."
                });
            }

            var doctor = await _context.Doctors
                .Where(d => d.UserId == userId && !d.IsDeleted)
                .Select(d => new GetDoctorDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    NormalizedName = d.NormalizedName,
                    Specialization = new DTOs.SharedDTO.GetItemsDto
                    {
                        Id = d.SpecializationId,
                        Name = d.Specialization.Name,
                        NormalizedName = d.Specialization.NormalizedName,
                    },
                    Description = d.Description,
                    SubscriptionRank = d.SubscriptionRank,
                    IraqiProvince = d.IraqiProvince,
                    IraqiProvinceName = d.IraqiProvince.GetDisplayName(),
                    IraqiProvinceNormalizedName = d.IraqiProvince.ToString(),
                    BirthDay = d.BirthDay,
                    ImageName = d.ImageName,
                    Location = d.Location,
                    PhoneNumber = d.PhoneNumber,
                    IsPubliclyVisible = d.IsPubliclyVisible,
                    UserId = d.UserId,
                })
                .FirstOrDefaultAsync();

            if (doctor == null)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "لا يوجد ملف طبيب مرتبط بهذا الحساب."
                });
            }

            return new OkObjectResult(new ResponseDto<GetDoctorDto>
            {
                Status = "Success",
                Code = 200,
                Message = "تم جلب ملف الطبيب بنجاح.",
                Data = doctor
            });
        }

        public async Task<IActionResult> UpdateMyProfileAsync(DoctorProfileUpdateDto form)
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
            {
                return new UnauthorizedObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 401,
                    Message = "يجب تسجيل الدخول أولاً."
                });
            }

            var doctorId = await _context.Doctors
                .Where(d => d.UserId == userId && !d.IsDeleted)
                .Select(d => (int?)d.Id)
                .FirstOrDefaultAsync();

            if (doctorId == null)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "لا يوجد ملف طبيب مرتبط بهذا الحساب."
                });
            }

            return await UpdateDoctorAsync(new DoctorUpdateDto
            {
                Id = doctorId.Value,
                Name = form.Name,
                NormalizedName = form.NormalizedName,
                SpecializationId = form.SpecializationId,
                Description = form.Description,
                IraqiProvince = form.IraqiProvince,
                ImageName = form.ImageName,
                BirthDay = form.BirthDay,
                PhoneNumber = form.PhoneNumber,
                Location = form.Location
            });
        }

        public async Task<IActionResult> LinkAccountAsync(int doctorId, LinkDoctorAccountDto form)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == doctorId && !d.IsDeleted);
            if (doctor == null)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "الدكتور غير موجود."
                });
            }

            var user = await _userManager.FindByIdAsync(form.UserId.ToString());
            if (user == null || user.IsDeleted)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "حساب المستخدم غير موجود."
                });
            }

            if (user.IsLocked || await _userManager.IsInRoleAsync(user, "SuperAdmin"))
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "لا يمكن ربط هذا الحساب بطبيب."
                });
            }

            var accountAlreadyLinked = await _context.Doctors
                .AnyAsync(d => d.Id != doctorId && d.UserId == form.UserId && !d.IsDeleted);
            if (accountAlreadyLinked)
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "هذا الحساب مرتبط بطبيب آخر."
                });
            }

            doctor.UserId = form.UserId;
            doctor.ModifierId = _load.GetCurrentUserId();
            doctor.ModifiedAt = DateTime.UtcNow;

            if (!await _userManager.IsInRoleAsync(user, "DoctorUser"))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, "DoctorUser");
                if (!roleResult.Succeeded)
                {
                    return new BadRequestObjectResult(new ResponseDto<string>
                    {
                        Status = "Error",
                        Code = 400,
                        Message = string.Join(" ", roleResult.Errors.Select(e => e.Description))
                    });
                }
            }

            await _context.SaveChangesAsync();
            return new OkObjectResult(new ResponseDto<string>
            {
                Status = "Success",
                Code = 200,
                Message = "تم ربط حساب الطبيب بنجاح."
            });
        }

        public async Task<IActionResult> UnlinkAccountAsync(int doctorId)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == doctorId && !d.IsDeleted);
            if (doctor == null)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "الدكتور غير موجود."
                });
            }

            doctor.UserId = null;
            doctor.ModifierId = _load.GetCurrentUserId();
            doctor.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<string>
            {
                Status = "Success",
                Code = 200,
                Message = "تم فصل حساب الطبيب بنجاح."
            });
        }

        public async Task<IActionResult> UpdateVisibilityAsync(int doctorId, DoctorVisibilityUpdateDto form)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == doctorId && !d.IsDeleted);
            if (doctor == null)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "الدكتور غير موجود."
                });
            }

            doctor.IsPubliclyVisible = form.IsPubliclyVisible;
            doctor.ModifierId = _load.GetCurrentUserId();
            doctor.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = form.IsPubliclyVisible
                    ? "تمت الموافقة على ظهور الطبيب للعامة."
                    : "تم إخفاء الطبيب عن العرض العام.",
                Data = new { doctor.Id, doctor.IsPubliclyVisible }
            });
        }
        public async Task<ActionResult<PaginationDto.PageResult<GetDoctorDto>>> GetListAsync(SearchDoctorDto form, int page = 1, int pageSize = 10)
        {
            try
            {
                if (page <= 0 || pageSize <= 0)
                {
                    return new BadRequestObjectResult(new ResponseDto<string>
                    {
                        Status = "Error",
                        Code = 400,
                        Message = "قيم الصفحة أو الحجم غير صحيحة.",
                        Data = null
                    });
                }
                var query = _context.Doctors
    .Where(d => !d.IsDeleted)
    .Where(d => form.Specialization == null || d.SpecializationId == form.Specialization)
    .Where(d => form.IraqiProvince == null || d.IraqiProvince == form.IraqiProvince)
    .Where(d => form.Id == null || d.Id == form.Id);

                if (!string.IsNullOrWhiteSpace(form.Name))
                {
                    query = query.Where(d => d.Name.Contains(form.Name) || d.NormalizedName.Contains(form.Name));
                }


                var totalItems = await query.CountAsync();

                if (totalItems == 0)
                {
                    return new NotFoundObjectResult(new ResponseDto<string>
                    {
                        Status = "Not Found",
                        Code = 404,
                        Message = "لا توجد بيانات للعرض!",
                        Data = null
                    });
                }

                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var docs = await query
                    .OrderBy(d => d.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(d => new GetDoctorDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        NormalizedName = d.NormalizedName,
                        Specialization = new DTOs.SharedDTO.GetItemsDto
                        {
                            Id = d.SpecializationId,
                            Name = d.Specialization.Name,
                            NormalizedName = d.Specialization.NormalizedName,
                        },
                        Description = d.Description,
                        SubscriptionRank = d.SubscriptionRank,
                        IraqiProvince = d.IraqiProvince,
                        IraqiProvinceName = d.IraqiProvince.GetDisplayName(),
                        IraqiProvinceNormalizedName = d.IraqiProvince.ToString(),
                        BirthDay = d.BirthDay,
                        ImageName = d.ImageName,
                        Location = d.Location,
                        PhoneNumber = d.PhoneNumber,
                        IsPubliclyVisible = d.IsPubliclyVisible,
                        UserId = d.UserId,
                    })
                    .ToListAsync();

                var result = new PaginationDto.PageResult<GetDoctorDto>(docs, totalItems, totalPages, page, pageSize);

                return new OkObjectResult(new ResponseDto<PaginationDto.PageResult<GetDoctorDto>>
                {
                    Status = "Success",
                    Code = 200,
                    Message = "تم جلب البيانات بنجاح!",
                    Data = result
                });
            }
            catch (DbUpdateException ex)
            {
                return new ObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = "خطأ في قاعدة البيانات!",
                    Data = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = "حدث خطأ غير متوقع!",
                    Data = ex.Message
                });
            }
        }

        public async Task<IActionResult> AddDoctorAsync(DoctorAddDto form)
        {
            try
            {
                var imageValidation = ValidateDoctorImage(form.ImageName);
                if (imageValidation != null) return imageValidation;

                var exists = await _context.Doctors.AnyAsync(d =>
                    d.Name.Contains(form.Name) &&
                    d.SpecializationId == form.SpecializationId &&
                    d.BirthDay == form.BirthDay);

                if (exists)
                {
                    return new BadRequestObjectResult(new ResponseDto<object>
                    {
                        Status = "Error",
                        Code = 400,
                        Message = "الدكتور موجود مسبقًا!",
                        Data = null
                    });
                }
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/DoctorImage");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(form.ImageName.FileName);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/DoctorImage", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await form.ImageName.CopyToAsync(stream);
                }

                var doctor = new Doctor
                {
                    Name = form.Name,
                    NormalizedName = form.NormalizedName,
                    SpecializationId = form.SpecializationId,
                    Description = form.Description,
                    IraqiProvince = form.IraqiProvince,
                    ImageName = fileName,
                    BirthDay = form.BirthDay,
                    Location = form.Location,
                    PhoneNumber = form.PhoneNumber,
                    CreatorId = _load.GetCurrentUserId(),
                };

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                return new OkObjectResult(new ResponseDto<object>
                {
                    Status = "Success",
                    Code = 200,
                    Message = "تمت إضافة الدكتور بنجاح!",
                    Data = new { doctor.Id }
                });
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Database update exception: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");

                var errorResponse = new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = ex.Message,
                };

                return new ObjectResult(errorResponse)
                {
                    StatusCode = 500
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected exception occurred: {ex.Message}");

                var errorResponse = new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = ex.Message
                };

                return new ObjectResult(errorResponse)
                {
                    StatusCode = 500
                };
            }
        }
        public async Task<IActionResult> UpdateDoctorAsync(DoctorUpdateDto form)
        {
            try
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == form.Id && !d.IsDeleted);
                if (doctor == null)
                {
                    return new NotFoundObjectResult(new ResponseDto<object>
                    {
                        Status = "Error",
                        Code = 404,
                        Message = "الدكتور غير موجود!",
                        Data = null
                    });
                }

                // Check for duplicates (excluding the same record)
                var duplicate = await _context.Doctors.AnyAsync(d =>
                    d.Id != form.Id &&
                    d.Name.Contains(form.Name) &&
                    d.SpecializationId == form.SpecializationId &&
                    d.BirthDay == form.BirthDay);

                if (duplicate)
                {
                    return new BadRequestObjectResult(new ResponseDto<object>
                    {
                        Status = "Error",
                        Code = 400,
                        Message = "دكتور بنفس المعلومات موجود مسبقًا!",
                        Data = null
                    });
                }

                doctor.Name = form.Name;
                doctor.NormalizedName = form.NormalizedName;
                doctor.SpecializationId = form.SpecializationId;
                doctor.Description = form.Description;
                doctor.IraqiProvince = form.IraqiProvince;
                doctor.BirthDay = form.BirthDay;
                doctor.PhoneNumber = form.PhoneNumber;
                doctor.Location = form.Location;
                doctor.ModifierId = _load.GetCurrentUserId();
                doctor.ModifiedAt = DateTime.Now;

                if (form.ImageName != null)
                {
                    var imageValidation = ValidateDoctorImage(form.ImageName);
                    if (imageValidation != null) return imageValidation;

                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/DoctorImage");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    // Optionally delete the old image
                    var oldImagePath = Path.Combine(folderPath, doctor.ImageName ?? "");
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(form.ImageName.FileName);
                    var filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await form.ImageName.CopyToAsync(stream);
                    }

                    doctor.ImageName = fileName;
                }

                _context.Doctors.Update(doctor);
                await _context.SaveChangesAsync();

                return new OkObjectResult(new ResponseDto<object>
                {
                    Status = "Success",
                    Code = 200,
                    Message = "تم تحديث معلومات الدكتور بنجاح!",
                    Data = null
                });
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Database update exception: {ex.Message}");

                var errorResponse = new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = ex.Message,
                };

                return new ObjectResult(errorResponse) { StatusCode = 500 };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected exception: {ex.Message}");

                var errorResponse = new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = ex.Message
                };

                return new ObjectResult(errorResponse) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                var doctor = await _context.Doctors.Where(d=>d.Id == id && !d.IsDeleted).FirstOrDefaultAsync();
                if(doctor != null)
                {
                    doctor.IsDeleted = true;
                    doctor.DeleterId = _load.GetCurrentUserId();
                    doctor.DeletedAt = DateTime.Now;
                    _context.Doctors.Update(doctor);
                    await _context.SaveChangesAsync();
                    return new OkObjectResult(new ResponseDto<string>
                    {
                        Status = "Success",
                        Code = 200,
                        Message = "تم الحذف بنجاح."
                    });
                }
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "المستخدم غير موجود."
                });
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Database update exception: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");

                var errorResponse = new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = ex.Message,
                };

                return new ObjectResult(errorResponse)
                {
                    StatusCode = 500
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected exception occurred: {ex.Message}");

                var errorResponse = new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = ex.Message
                };

                return new ObjectResult(errorResponse)
                {
                    StatusCode = 500
                };
            }
        }

        private static BadRequestObjectResult? ValidateDoctorImage(IFormFile? image)
        {
            if (image == null || image.Length == 0)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "يرجى اختيار صورة طبيب صالحة.",
                    Data = null
                });
            }

            const long maxFileSize = 5 * 1024 * 1024;
            var extension = Path.GetExtension(image.FileName);
            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };
            if (image.Length > maxFileSize || !allowedExtensions.Contains(extension))
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "الصورة يجب أن تكون بصيغة JPG أو PNG أو WEBP وبحجم لا يتجاوز 5MB.",
                    Data = null
                });
            }

            return null;
        }
    }
}
