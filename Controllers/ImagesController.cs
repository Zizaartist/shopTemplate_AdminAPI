using ApiClick.Models;
using ApiClick.Models.EnumModels;
using ApiClick.StaticValues;
using ImageMagick;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using static ApiClick.StaticValues.ImageSpecs;

namespace ApiClick.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly IWebHostEnvironment _appEnvironment;

        public ImagesController(IWebHostEnvironment appEnvironment)
        {
            _appEnvironment = appEnvironment;
        }

        [Route("{_type}")]
        [HttpPost]
        public ActionResult<string> PostImage(IFormFile uploadedFile, ImageType _type)
        {
            if (uploadedFile == null)
            {
                return BadRequest();
            }

            //Проверить все ли являются изображениями и имеют реальный размер
            if (uploadedFile.Length <= 0)
            {
                return BadRequest(); //probably wrong code
            }
            if (!uploadedFile.ContentType.Contains("image"))
            {
                return BadRequest(); //probably wrong code    
            }

            // путь к папке Files, ЗАМЕНИТЬ Path.GetTempFileName на более надежный генератор
            string newFileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".jpg";

            var folderPath = _appEnvironment.WebRootPath + "/Images/";
            var fullNewFilePath = folderPath + newFileName;

            using (var fileStream = new FileStream(fullNewFilePath, FileMode.Create))
            {
                //Буфер для обработки файла без лишних действий (чтение, запись)
                using (var resultStream = new MemoryStream())
                {
                    //Читаем из полученного файла
                    using (var readStream = uploadedFile.OpenReadStream())
                    {
                        using (var image = new MagickImage(readStream))
                        {
                            //Изначальная максимальная величина
                            var maxValue = 0;
                            if (image.Height >= image.Width) maxValue = image.Height;
                            else maxValue = image.Width;

                            if (maxValue > ImageSpecs.ImageTypeToMax[_type]) maxValue = ImageSpecs.ImageTypeToMax[_type];

                            //Конфигурация ресайзера
                            var size = new MagickGeometry(maxValue);
                            size.IgnoreAspectRatio = false; //Всегда сохраняем aspectRatio

                            //Применяем ресайз и записываем в буфер
                            image.Resize(size);
                            image.Format = MagickFormat.Jpg;
                            image.Write(resultStream);

                            resultStream.Position = 0; //Сбрасываем каретку, автоматически это не делается

                            //Компрессируем буфер, вместо файла
                            ImageOptimizer optimizer = new ImageOptimizer();
                            optimizer.Compress(resultStream);
                        }
                    }
                    //Записываем буфер в виде реального файла
                    resultStream.CopyTo(fileStream);
                }
            }
            return newFileName;
        }
    }
}
