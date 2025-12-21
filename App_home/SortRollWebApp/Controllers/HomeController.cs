using ClosedXML.Excel;

using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using SortRollWebApp.Data;
using SortRollWebApp.Models;
using SortRollWebApp.Models.Entities;
using SortRollWebApp.Services;

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SortRollWebApp.Controllers
{
    public class HomeController : Controller
    {

        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        private GlobalContext _globalContext = new GlobalContext();
        private PassRollingParameters _passRollingParameters = new PassRollingParameters();

        public IActionResult Privacy()
        {
            return View();
        }

        // Главная страница
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CheckDatabaseConnection()
        {
            try
            {
                // Пытаемся выполнить простой запрос к базе данных
                var factoriesCount = _db.Factories.Count();
                return Json(new { connected = true });
            }
            catch (Exception ex)
            {
                // Логируем ошибку, если нужно
                // _logger.LogError(ex, "Ошибка подключения к базе данных");
                return Json(new { connected = false });
            }
        }

        // Страница подготовки
        // Методы для работы с заводами
        [HttpPost]
        public async Task<IActionResult> SaveFactory(int id, string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest("Название не может быть пустым");

            if (id == 0)
            {
                _db.Factories.Add(new Factory { Name = name });
            }
            else
            {
                var factory = await _db.Factories.FindAsync(id);
                if (factory == null)
                    return NotFound();

                factory.Name = name;
                _db.Factories.Update(factory);
            }

            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFactory(int id)
        {
            var factory = await _db.Factories.FindAsync(id);
            if (factory == null)
                return NotFound();

            _db.Factories.Remove(factory);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public JsonResult GetFactories()
        {
            var factories = _db.Factories
                .Select(f => new { id = f.Id, name = f.Name })
                .ToList();
            return Json(factories);
        }

        // Методы для работы с станами
        [HttpPost]
        public async Task<IActionResult> SaveMill(int id, string name, int factoryId)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest("Название не может быть пустым");

            if (factoryId == 0)
                return BadRequest("Не выбран завод");

            if (id == 0)
            {
                _db.RollingMills.Add(new RollingMill { Name = name, IdFactory = factoryId });
            }
            else
            {
                var mill = await _db.RollingMills.FindAsync(id);
                if (mill == null)
                    return NotFound();

                mill.Name = name;
                mill.IdFactory = factoryId;
                _db.RollingMills.Update(mill);
            }

            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMill(int id)
        {
            var mill = await _db.RollingMills.FindAsync(id);
            if (mill == null)
                return NotFound();

            _db.RollingMills.Remove(mill);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public JsonResult GetMill(int id)
        {
            var mill = _db.RollingMills
                .Where(m => m.Id == id)
                .Select(m => new { id = m.Id, name = m.Name, idFactory = m.IdFactory })
                .FirstOrDefault();
            return Json(mill);
        }

        // Методы для работы с профилями
        [HttpPost]
        public async Task<IActionResult> SaveSection(int id, string name, string description, int millId)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest("Название не может быть пустым");

            if (millId == 0)
                return BadRequest("Не выбран стан");

            if (id == 0)
            {
                _db.SteelSections.Add(new SteelSection
                {
                    Name = name,
                    Description = description,
                    IdRollingMill = millId
                });
            }
            else
            {
                var section = await _db.SteelSections.FindAsync(id);
                if (section == null)
                    return NotFound();

                section.Name = name;
                section.Description = description;
                section.IdRollingMill = millId;
                _db.SteelSections.Update(section);
            }

            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSection(int id)
        {
            var section = await _db.SteelSections.FindAsync(id);
            if (section == null)
                return NotFound();

            _db.SteelSections.Remove(section);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public JsonResult GetSection(int id)
        {
            var section = _db.SteelSections
                .Where(s => s.Id == id)
                .Select(s => new {
                    id = s.Id,
                    name = s.Name,
                    description = s.Description,
                    idRollingMill = s.IdRollingMill
                })
                .FirstOrDefault();
            return Json(section);
        }

        public IActionResult Prepare()
        {
            ViewBag.Factories = _db.Factories
                .Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name })
                .ToList();
            return View();
        }

        [HttpGet]
        public JsonResult GetRollingMills(int id)
        {
            var mills = _db.RollingMills
                .Where(m => m.IdFactory == id)
                .Select(m => new { id = m.Id, name = m.Name })
                .ToList();
            return Json(mills);
        }

        [HttpGet]
        public JsonResult GetSteelSections(int id)
        {
            var sections = _db.SteelSections
                .Where(s => s.IdRollingMill == id)
                .Select(s => new { id = s.Id, name = s.Name })
                .ToList();
            return Json(sections);
        }

        [HttpGet]
        public JsonResult GetRollingStands(int millId)
        {
            var stands = _db.RollingStands
                .Where(s => s.IdRollingMill == millId)
                .OrderBy(s => s.Number)
                .ToList();

            return Json(stands);
        }

        [HttpPost]
        public async Task<IActionResult> SaveRollingStand([FromBody] RollingStand stand)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (stand.Id == 0)
                    {
                        _db.RollingStands.Add(stand);
                    }
                    else
                    {
                        _db.RollingStands.Update(stand);
                    }
                    await _db.SaveChangesAsync();

                    return Json(stand);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { error = ex.Message });
                }
            }

            return BadRequest(new
            {
                errors = ModelState.ToDictionary(
                k => k.Key,
                v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray())
            });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRollingStand(int id)
        {
            var stand = await _db.RollingStands.FindAsync(id);
            if (stand != null)
            {
                _db.RollingStands.Remove(stand);
                await _db.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        public IActionResult GetNewStandRow(int idMill)
        {
            var newStand = new RollingStand { IdRollingMill = idMill };
            return PartialView("_RollingStandRow", newStand);
        }

        [HttpPost]
        public IActionResult UpdateRollingStand([FromBody] RollingStand stand)
        {
            var existingStand = _db.RollingStands.FirstOrDefault(rs => rs.Id == stand.Id);
            if (existingStand != null)
            {
                existingStand.Number = stand.Number;
                existingStand.DB = stand.DB;
                existingStand.DSH = stand.DSH;
                existingStand.MU = stand.MU;
                existingStand.FPOD = stand.FPOD;
                existingStand.LKL = stand.LKL;
                existingStand.AKL = stand.AKL;
                existingStand.IR = stand.IR;
                existingStand.ETA = stand.ETA;
                existingStand.NZ = stand.NZ;
                existingStand.NNOM = stand.NNOM;
                existingStand.NDVN = stand.NDVN;
                existingStand.NDVMIN = stand.NDVMIN;
                existingStand.NDVMAX = stand.NDVMAX;
                existingStand.PP = stand.PP;
                existingStand.PDOP = stand.PDOP;
                existingStand.MDOP = stand.MDOP;
                existingStand.C = stand.C;
            }
            return Json(new { success = true });
        }

        public IActionResult Passes(int factoryId, int millId, int sectionId)
        {
            // Загружаем названия по id
            var factory = _db.Factories.Find(factoryId);
            var mill = _db.RollingMills.Find(millId);
            var section = _db.SteelSections.Find(sectionId);

            ViewBag.FactoryName = factory?.Name;
            ViewBag.MillName = mill?.Name;
            ViewBag.SectionName = section?.Name;

            // Также передаем id, чтобы использовать их для загрузки данных таблицы Pass
            ViewBag.FactoryId = factoryId;
            ViewBag.MillId = millId;
            ViewBag.SectionId = sectionId;

            return View();
        }

        // Страница калибровки
        [HttpGet]
        public JsonResult GetPasses(int sectionId)
        {
            var passes = _db.Passes
                .Where(p => p.IdSteelSection == sectionId)
                .OrderBy(p => p.N)
                .ToList();

            return Json(passes);
        }

        [HttpPost]
        public async Task<IActionResult> SavePass([FromBody] Pass pass)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (pass.Id == 0)
                    {
                        _db.Passes.Add(pass);
                    }
                    else
                    {
                        _db.Passes.Update(pass);
                    }
                    await _db.SaveChangesAsync();

                    return Json(pass);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { error = ex.Message });
                }
            }

            return BadRequest(new
            {
                errors = ModelState.ToDictionary(
                k => k.Key,
                v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray())
            });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePass(int id)
        {
            var pass = await _db.Passes.FindAsync(id);
            if (pass != null)
            {
                _db.Passes.Remove(pass);
                await _db.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        [HttpGet]
        public JsonResult GetInitialParams(int sectionId)
        {
            var initialParams = _db.InitialParameters
                .Where(p => p.IdSteelSection == sectionId)
                .ToList();
            return Json(initialParams);
        }

        [HttpPost]
        public async Task<IActionResult> SaveInitialParam([FromBody] InitialParameters param)
        {
            ModelState.Remove("SteelSection");

            if (ModelState.IsValid)
            {
                try
                {
                    if (param.Id == 0)
                    {
                        _db.InitialParameters.Add(param);
                    }
                    else
                    {
                        _db.InitialParameters.Update(param);
                    }
                    await _db.SaveChangesAsync();
                    return Json(param);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { error = ex.Message });
                }
            }

            return BadRequest(new
            {
                errors = ModelState.ToDictionary(
                k => k.Key,
                v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray())
            });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteInitialParam(int id)
        {
            var param = await _db.InitialParameters.FindAsync(id);
            if (param != null)
            {
                _db.InitialParameters.Remove(param);
                await _db.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        // Методы для работы со сталями
        [HttpGet]
        public JsonResult GetSteels()
        {
            var steels = _db.Steels.ToList();
            return Json(steels);
        }

        [HttpPost]
        public async Task<IActionResult> SaveSteel([FromBody] Steel steel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (steel.Id == 0)
                    {
                        _db.Steels.Add(steel);
                    }
                    else
                    {
                        _db.Steels.Update(steel);
                    }
                    await _db.SaveChangesAsync();
                    return Json(steel);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { error = ex.Message });
                }
            }

            return BadRequest(new
            {
                errors = ModelState.ToDictionary(
                k => k.Key,
                v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray())
            });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSteel(int id)
        {
            var steel = await _db.Steels.FindAsync(id);
            if (steel != null)
            {
                _db.Steels.Remove(steel);
                await _db.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        //public IActionResult Results(int factoryId, int millId, int sectionId, int initialParamId, int steelId)
        //{
        //    // Загружаем все необходимые данные
        //    var factory = _db.Factories.Find(factoryId);
        //    var mill = _db.RollingMills.Find(millId);
        //    var section = _db.SteelSections.Find(sectionId);
        //    var initialParam = _db.InitialParameters.Find(initialParamId);
        //    var steel = _db.Steels.Find(steelId);
        //    var stands = _db.RollingStands.Where(s => s.IdRollingMill == millId).ToList();
        //    var passes = _db.Passes.Where(p => p.IdSteelSection == sectionId).ToList();

        //    // Передаем данные в представление
        //    ViewBag.FactoryName = factory?.Name;
        //    ViewBag.MillName = mill?.Name;
        //    ViewBag.SectionName = section?.Name;
        //    ViewBag.InitialParam = initialParam;
        //    ViewBag.Steel = steel;
        //    ViewBag.Stands = stands;
        //    ViewBag.Passes = passes;

        //    return View();
        //}

        // Страница результатов

        [HttpGet]
        public IActionResult Results(int factoryId, int millId, int sectionId, int initialParamId, int steelId)
        {
            // Загрузка данных из БД
            var factory = _db.Factories.Include(f => f.RollingMills).FirstOrDefault(f => f.Id == factoryId);
            var mill = _db.RollingMills.Include(m => m.SteelSections).FirstOrDefault(m => m.Id == millId);
            var section = _db.SteelSections.FirstOrDefault(s => s.Id == sectionId);
            var initialParams = _db.InitialParameters.FirstOrDefault(ip => ip.Id == initialParamId);
            var steel = _db.Steels.FirstOrDefault(s => s.Id == steelId);

            // Получаем калибровку (проходы) для выбранного профиля
            var passes = _db.Passes
                .Where(p => p.IdSteelSection == sectionId)
                .OrderBy(p => p.N)
                .ToList();

            // Получаем параметры клетей
            var rollingStands = _db.RollingStands
                .Where(rs => rs.IdRollingMill == millId)
                .OrderBy(rs => rs.Number)
                .ToList();

            // Создаем и заполняем GlobalContext
            var globalContext = new GlobalContext
            {
                Factory = factory,
                RollingMill = mill,
                SteelSection = section,
                InitialParameters = initialParams,
                Steel = steel,
                GlobalRollingParameters = new GlobalRollingParameters
                {
                    NPR = passes.Count // Количество проходов
                },
                Passes = passes.Select((p, index) => new PassContext
                {
                    Pass = p,
                    RollingStand = rollingStands.FirstOrDefault(rs => rs.Number == p.N),
                    PassRollingParameters = new PassRollingParameters()
                }).ToList()
            };

            return View(globalContext);
        }

        [HttpPost]
        public IActionResult CalculateResults([FromBody] CalculationRequest request)
        {
            // Загрузка данных из БД
            var factory = _db.Factories.Include(f => f.RollingMills).FirstOrDefault(f => f.Id == request.FactoryId);
            var mill = _db.RollingMills.Include(m => m.SteelSections).FirstOrDefault(m => m.Id == request.MillId);
            var section = _db.SteelSections.FirstOrDefault(s => s.Id == request.SectionId);
            var initialParams = _db.InitialParameters.FirstOrDefault(ip => ip.Id == request.InitialParamId);
            var steel = _db.Steels.FirstOrDefault(s => s.Id == request.SteelId);

            // Получаем калибровку (проходы) для выбранного профиля
            var passes = _db.Passes
                .Where(p => p.IdSteelSection == request.SectionId)
                .OrderBy(p => p.N)
                .ToList();

            // Получаем параметры клетей
            var rollingStands = _db.RollingStands
                .Where(rs => rs.IdRollingMill == request.MillId)
                .OrderBy(rs => rs.Number)
                .ToList();

            // Создаем список проходов с индексацией, начинающейся с 1
            var passesList = new List<PassContext>(passes.Count + 1);

            // Добавляем "заглушку" в начало списка (индекс 0 не используется)
            passesList.Add(new PassContext());

            // Добавляем проходы, начиная с индекса 1
            foreach (var pass in passes)
            {
                passesList.Add(new PassContext
                {
                    Pass = pass,
                    RollingStand = rollingStands.FirstOrDefault(rs => rs.Number == pass.N),
                    PassRollingParameters = new PassRollingParameters()
                });
            }

            // Создаем и заполняем GlobalContext
            var globalContext = new GlobalContext
            {
                Factory = factory,
                RollingMill = mill,
                SteelSection = section,
                InitialParameters = initialParams,
                Steel = steel,
                GlobalRollingParameters = new GlobalRollingParameters
                {
                    NPR = passes.Count // Количество проходов
                },
                Passes = passesList
            };

            // Выполняем расчет
            var calculator = new RollingCalculator(globalContext);
            try
            {
                calculator.ParZeroData();
                if (request.CalculationMethod == "Domain")
                {
                    calculator.Domain();
                }
                else
                {
                    calculator.DomainSP();
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }

            // Формируем результаты с проверкой на специальные значения
            var results = new
            {
                success = true,
                passes = globalContext.Passes.Skip(1).Select((p, index) => new
                {
                    passNumber = index + 1,
                    schemaCaliber = p.Pass.SchemaCaliber,
                    hvr = SafeDouble(p.PassRollingParameters.HVR),
                    bk = SafeDouble(p.PassRollingParameters.BK),
                    s = SafeDouble(p.Pass.S),
                    h1 = SafeDouble(p.Pass.H1),
                    b1 = SafeDouble(p.Pass.B1),
                    wr = SafeDouble(p.PassRollingParameters.WR),
                    lod = SafeDouble(p.PassRollingParameters.LOD),
                    kvitr = SafeDouble(p.PassRollingParameters.KVITR),
                    ugzax = SafeDouble(p.PassRollingParameters.UGZAX),
                    dd = SafeDouble(p.PassRollingParameters.DD),
                    dk = SafeDouble(p.PassRollingParameters.DK),
                    v1 = SafeDouble(p.PassRollingParameters.V1),
                    nr = SafeDouble(p.PassRollingParameters.NR),
                    ndvr = SafeDouble(p.PassRollingParameters.NDVR),
                    tsr = SafeDouble(p.PassRollingParameters.TSR),
                    p1p = SafeDouble(p.PassRollingParameters.P1P),
                    p1spp = SafeDouble(p.PassRollingParameters.P1SPP),
                    rp = SafeDouble(p.PassRollingParameters.RP),
                    rspp = SafeDouble(p.PassRollingParameters.RSPP),
                    mprp = SafeDouble(p.PassRollingParameters.MPRP),
                    mprspp = SafeDouble(p.PassRollingParameters.MPRSPP),
                    nprp = SafeDouble(p.PassRollingParameters.NPRP),
                    npspp = SafeDouble(p.PassRollingParameters.NPSPP),
                    vmin = SafeDouble(p.PassRollingParameters.VMIN),
                    vmax = SafeDouble(p.PassRollingParameters.VMAX)
                }).ToList()
            };

            return new JsonResult(results, new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
            });
        }

        private double? SafeDouble(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return null;

            return value;
        }

        private double? SafeDouble(double? value)
        {
            if (!value.HasValue)
                return null;

            if (double.IsNaN(value.Value) || double.IsInfinity(value.Value))
                return null;

            return value;
        }

        public class CalculationRequest
        {
            public int FactoryId { get; set; }
            public int MillId { get; set; }
            public int SectionId { get; set; }
            public int InitialParamId { get; set; }
            public int SteelId { get; set; }
            public string CalculationMethod { get; set; } // Domain или DomainSP
        }


        [HttpPost]
        public IActionResult ExportToExcel([FromBody] ExcelExportRequest request)
        {
            // Загрузка данных из БД
            var factory = _db.Factories.Include(f => f.RollingMills).FirstOrDefault(f => f.Id == request.FactoryId);
            var mill = _db.RollingMills.Include(m => m.SteelSections).FirstOrDefault(m => m.Id == request.MillId);
            var section = _db.SteelSections.FirstOrDefault(s => s.Id == request.SectionId);
            var initialParams = _db.InitialParameters.FirstOrDefault(ip => ip.Id == request.InitialParamId);
            var steel = _db.Steels.FirstOrDefault(s => s.Id == request.SteelId);

            // Получаем калибровку (проходы) для выбранного профиля
            var passes = _db.Passes
                .Where(p => p.IdSteelSection == request.SectionId)
                .OrderBy(p => p.N)
                .ToList();

            // Получаем параметры клетей
            var rollingStands = _db.RollingStands
                .Where(rs => rs.IdRollingMill == request.MillId)
                .OrderBy(rs => rs.Number)
                .ToList();

            // Создаем список проходов с индексацией, начинающейся с 1
            var passesList = new List<PassContext>(passes.Count + 1);

            // Добавляем "заглушку" в начало списка (индекс 0 не используется)
            passesList.Add(new PassContext());

            // Добавляем проходы, начиная с индекса 1
            foreach (var pass in passes)
            {
                passesList.Add(new PassContext
                {
                    Pass = pass,
                    RollingStand = rollingStands.FirstOrDefault(rs => rs.Number == pass.N),
                    PassRollingParameters = new PassRollingParameters()
                });
            }

            // Создаем и заполняем GlobalContext
            var globalContext = new GlobalContext
            {
                Factory = factory,
                RollingMill = mill,
                SteelSection = section,
                InitialParameters = initialParams,
                Steel = steel,
                GlobalRollingParameters = new GlobalRollingParameters
                {
                    NPR = passes.Count // Количество проходов
                },
                Passes = passesList
            };

            // Выполняем расчет
            var calculator = new RollingCalculator(globalContext);
            try
            {
                if (request.CalculationMethod == "Domain")
                {
                    calculator.Domain();
                }
                else
                {
                    calculator.DomainSP();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }

            // Создаем Excel-документ
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Результаты расчета");

            // Стили
            var headerStyle = ws.Style;
            headerStyle.Fill.BackgroundColor = XLColor.NoColor;
            headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerStyle.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            var tableHeaderStyle = ws.Style;
            tableHeaderStyle.Fill.BackgroundColor = XLColor.NoColor;
            tableHeaderStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            tableHeaderStyle.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            tableHeaderStyle.Border.OutsideBorder = XLBorderStyleValues.Thin;
            tableHeaderStyle.Border.InsideBorder = XLBorderStyleValues.Thin;

            var cellStyle = ws.Style;
            cellStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cellStyle.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cellStyle.Fill.BackgroundColor = XLColor.NoColor;
            cellStyle.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cellStyle.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Заголовок отчета
            ws.Cell(1, 1).Value = "ОТЧЕТ ПО РАСЧЕТУ ПРОЦЕССА СОРТОВОЙ ПРОКАТКИ";
            ws.Range(1, 1, 1, 21).Merge();
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 16;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Информация о параметрах расчета
            ws.Cell(2, 1).Value = $"Завод: {factory.Name}";
            ws.Range(2, 1, 2, 7).Merge();

            ws.Cell(3, 1).Value = $"Стан: {mill.Name}";
            ws.Range(3, 1, 3, 7).Merge();

            ws.Cell(4, 1).Value = $"Профиль: {section.Name}";
            ws.Range(4, 1, 4, 7).Merge();

            ws.Cell(5, 1).Value = $"Метод расчета: " +
                (request.CalculationMethod == "Domain" ? "Метод кафедры ОМД УГТУ" : "Метод соответственной полосы");
            ws.Range(5, 1, 5, 7).Merge();

            ws.Cell(6, 1).Value = $"Марка стали: {steel.Name}";
            ws.Range(6, 1, 6, 7).Merge();

            // Заголовок таблицы
            ws.Cell(8, 1).Value = "Таблица калибровки валков";
            ws.Range(8, 1, 8, 21).Merge();
            ws.Cell(8, 1).Style.Font.Bold = true;
            ws.Cell(8, 1).Style.Font.FontSize = 14;
            ws.Cell(8, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Заголовки таблицы
            var headers = new[] {
        "№ клети", "Форма калибра", "Глубина вреза, мм", "Ширина вреза, мм", "Зазор, мм",
        "Высота раската, мм", "Ширина раската, мм", "Площадь сечения, мм²", "Длина, мм", "Коэф. вытяжки",
        "Угол захвата, °", "Диаметр по буртам, мм", "Катающий диаметр, мм", "Скорость прокатки, м/с",
        "Частота валков, об/мин", "Частота двигателя, об/мин", "Температура, °C", "Усилие, МН",
        "Макс. реакция, МН", "Момент прокатки, кН·м", "Мощность, кВт"
    };

            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(9, i + 1).Value = headers[i];
                ws.Cell(9, i + 1).Style = tableHeaderStyle;
            }

            // Данные таблицы
            int row = 10;
            bool isDomain = request.CalculationMethod == "Domain";

            for (int i = 1; i <= globalContext.GlobalRollingParameters.NPR; i++)
            {
                var pass = globalContext.Passes[i];
                var passRolling = pass.PassRollingParameters;

                ws.Cell(row, 1).Value = pass.Pass.N;
                ws.Cell(row, 2).Value = GetSchemaCaliberName(pass.Pass.SchemaCaliber);
                ws.Cell(row, 3).Value = SafeDouble(passRolling.HVR);
                ws.Cell(row, 4).Value = SafeDouble(passRolling.BK);
                ws.Cell(row, 5).Value = SafeDouble(pass.Pass.S);
                ws.Cell(row, 6).Value = SafeDouble(pass.Pass.H1);
                ws.Cell(row, 7).Value = SafeDouble(pass.Pass.B1);
                ws.Cell(row, 8).Value = SafeDouble(passRolling.WR);
                ws.Cell(row, 9).Value = SafeDouble(passRolling.LOD);
                ws.Cell(row, 10).Value = SafeDouble(passRolling.KVITR);
                ws.Cell(row, 11).Value = SafeDouble(passRolling.UGZAX);
                ws.Cell(row, 12).Value = SafeDouble(passRolling.DD);
                ws.Cell(row, 13).Value = SafeDouble(passRolling.DK);
                ws.Cell(row, 14).Value = SafeDouble(passRolling.V1);
                ws.Cell(row, 15).Value = SafeDouble(passRolling.NR);
                ws.Cell(row, 16).Value = SafeDouble(passRolling.NDVR);
                ws.Cell(row, 17).Value = SafeDouble(passRolling.TSR);
                ws.Cell(row, 18).Value = isDomain ? SafeDouble(passRolling.P1P) : SafeDouble(passRolling.P1SPP);
                ws.Cell(row, 19).Value = isDomain ? SafeDouble(passRolling.RP) : SafeDouble(passRolling.RSPP);
                ws.Cell(row, 20).Value = isDomain ? SafeDouble(passRolling.MPRP) : SafeDouble(passRolling.MPRSPP);
                ws.Cell(row, 21).Value = isDomain ? SafeDouble(passRolling.NPRP) : SafeDouble(passRolling.NPSPP);

                // Применяем стиль к строке
                for (int col = 1; col <= 21; col++)
                {
                    ws.Cell(row, col).Style = cellStyle;
                }

                row++;
            }

            // Автоподбор ширины столбцов
            ws.Columns().AdjustToContents();

            // Добавляем график, если он был передан
            if (!string.IsNullOrEmpty(request.ChartImageBase64) && request.ChartImageBase64.StartsWith("image/png;base64,"))
            {
                try
                {
                    // Пропускаем префикс "image/png;base64,"
                    var base64Data = request.ChartImageBase64.Substring("image/png;base64,".Length);
                    var imageBytes = Convert.FromBase64String(base64Data);

                    using var imageStream = new MemoryStream(imageBytes);
                    var picture = ws.AddPicture(imageStream);

                    // Позиционируем график под таблицей
                    picture.MoveTo(ws.Cell(row + 2, 1));
                    picture.Width = 800;
                    picture.Height = 400;

                    // Добавляем заголовок для графика
                    ws.Cell(row + 1, 1).Value = "График скоростного режима";
                    ws.Range(row + 1, 1, row + 1, 10).Merge();
                    ws.Cell(row + 1, 1).Style.Font.Bold = true;
                    ws.Cell(row + 1, 1).Style.Font.FontSize = 14;
                }
                catch
                {
                    // Если не удалось добавить изображение, пропускаем
                }
            }

            // Формируем файл
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            // Возвращаем файл
            return File(
                stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Результаты_расчета_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            );
        }

        private string GetSchemaCaliberName(int schemaCode)
        {
            // Отображение названий форм калибров в зависимости от кода
            var schemaNames = new Dictionary<int, string>
            {
                { 0, "Гладкая бочка" },
                { 1, "Квадрат" },
                { 2, "Круглый" },
                { 3, "Ромбический" },
                { 4, "Овальный" },
                { 5, "Плоский овал" },
                { 6, "Шестиугольный" },
                { 7, "Диагональный квадрат" },
                { 8, "Ящичный" }
            };

            return schemaNames.TryGetValue(schemaCode, out var name) ? name : $"Неизвестно ({schemaCode})";
        }

        public class ExcelExportRequest
        {
            public int FactoryId { get; set; }
            public int MillId { get; set; }
            public int SectionId { get; set; }
            public int InitialParamId { get; set; }
            public int SteelId { get; set; }
            public string CalculationMethod { get; set; } // "Domain" или "DomainSP"
            public string ChartImageBase64 { get; set; } // Base64 представление графика
        }
    }
}