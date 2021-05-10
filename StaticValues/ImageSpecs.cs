using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiClick.StaticValues
{
    public class ImageSpecs
    {
        public const int AD_BANNER_MAX = 700;
        public const int CATEGORY_IMAGE_MAX = 400;
        public const int PRODUCT_IMAGE_MAX = 400;

        public enum ImageType 
        {
            adBanner,
            categoryImage,
            productImage
        }
        
        /// <summary>
        /// Получаем максимальное значение бОльшей стороны изображения
        /// </summary>
        public static Dictionary<ImageType, int> ImageTypeToMax = new Dictionary<ImageType, int>() 
        {
            { ImageType.adBanner, AD_BANNER_MAX },
            { ImageType.categoryImage, CATEGORY_IMAGE_MAX },
            { ImageType.productImage, PRODUCT_IMAGE_MAX }
        };
    }
}
