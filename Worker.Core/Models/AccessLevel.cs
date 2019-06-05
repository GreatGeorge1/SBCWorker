using System;
using System.Collections.Generic;
using System.Text;

namespace Worker.Core.Models
{
    public enum AccessLevel
    {
        /// <summary>
        ///     Первый уровень доступа
        /// </summary>
        Employee = 1,

        /// <summary>
        ///     Второй уровень доступа
        /// </summary>
        Security=2,

        /// <summary>
        ///     Третий уровень доступа
        /// </summary>
        Administration=3
    }
}
