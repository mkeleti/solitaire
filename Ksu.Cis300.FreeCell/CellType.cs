/* CellType.cs
 * Author: Rod Howell
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ksu.Cis300.FreeCell
{
    /// <summary>
    /// An enumeration of cell types.
    /// </summary>
    public enum CellType
    {
        /// <summary>
        /// A free cell.
        /// </summary>
        FreeCell,

        /// <summary>
        /// A home cell.
        /// </summary>
        HomeCell,

        /// <summary>
        /// A tableau cell
        /// </summary>
        TableauCell
    }
}
