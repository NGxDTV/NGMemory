using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGMemory
{
    public class Constants
    {
        public const uint PROCESS_ALL_ACCESS = (uint)(0x000F0000L | 0x00100000L | 0xFFF);
        public const uint PROCESS_QUERY_INFORMATION = 0x0400;
        public const uint PROCESS_VM_READ = 0x0010;
        public const uint PROCESS_VM_WRITE = 0x0020;
        public const uint PROCESS_VM_OPERATION = 0x0008;
        public const uint PROCESS_CREATE_THREAD = 0x0002;
        public const uint PROCESS_DUP_HANDLE = 0x0040;
        public const uint PROCESS_SET_INFORMATION = 0x0200;
        public const uint PROCESS_SET_QUOTA = 0x0100;
        public const uint PROCESS_SUSPEND_RESUME = 0x0800;
        public const uint PROCESS_TERMINATE = 0x0001;

        public const uint MEM_COALESCE_PLACEHOLDERS = 0x00000001;
        public const uint MEM_PRESERVE_PLACEHOLDER = 0x00000002;
        public const uint MEM_FREE = 0x10000;
        public const uint MEM_PRIVATE = 0x20000;
        public const uint MEM_MAPPED = 0x40000;
        public const uint MEM_RESET = 0x80000;
        public const uint MEM_RESET_UNDO = 0x1000000;
        public const uint MEM_IMAGE = 0x1000000;
        public const uint MEM_WRITE_WATCH = 0x00200000;
        public const uint MEM_PHYSICAL = 0x00400000;
        public const uint MEM_TOP_DOWN = 0x00100000;
        public const uint MEM_LARGE_PAGES = 0x20000000;
        public const uint MEM_COMMIT = 0x1000;
        public const uint MEM_RESERVE = 0x2000;
        public const uint MEM_DECOMMIT = 0x4000;
        public const uint MEM_RELEASE = 0x8000;

        public const uint PAGE_NOACCESS = 0x01;
        public const uint PAGE_READONLY = 0x02;
        public const uint PAGE_READWRITE = 0x04;
        public const uint PAGE_WRITECOPY = 0x08;
        public const uint PAGE_EXECUTE_READ = 0x20;
        public const uint PAGE_EXECUTE_READWRITE = 0x40;
        public const uint PAGE_EXECUTE_WRITECOPY = 0x80;
        public const uint PAGE_GUARD = 0x100;
        public const uint PAGE_NOCACHE = 0x200;
        public const uint PAGE_WRITECOMBINE = 0x400;

        public const uint CONTEXT_ALL = 0x0010003F;
        public const uint THREAD_ALL_ACCESS = 0x1F03FF;
        public const uint DBG_CONTINUE = 0x00010002;
        public const uint DBG_EXCEPTION_NOT_HANDLED = 0x80010001;
        public const uint EXCEPTION_BREAKPOINT = 0x80000003;
        public const uint CONTEXT_DEBUG_REGISTERS = 0x00000010;
        public const uint EXCEPTION_SINGLE_STEP = 0x80000004;

        public const uint EXCEPTION_DEBUG_EVENT = 1;
        public const uint CREATE_THREAD_DEBUG_EVENT = 2;
        public const uint CREATE_PROCESS_DEBUG_EVENT = 3;
        public const uint EXIT_THREAD_DEBUG_EVENT = 4;
        public const uint EXIT_PROCESS_DEBUG_EVENT = 5;
        public const uint LOAD_DLL_DEBUG_EVENT = 6;
        public const uint UNLOAD_DLL_DEBUG_EVENT = 7;
        public const uint OUTPUT_DEBUG_STRING_EVENT = 8;
        public const uint RIP_EVENT = 9;
        public const uint THREAD_EVENT = 10;
        public const uint EXCEPTION_EVENT = 11;
        public const uint PROCESS_EVENT = 12;

        public const int BM_GETCHECK = 0x00F0;
        public const int BM_SETSTATE = 0x00F3;
        public const int BM_CLICK = 0x00F5;
        public const int BM_SETCHECK = 0x00F1;
        public const int BM_GETSTATE = 0x00F2;
        public const int BM_SETIMAGE = 0x00F7;

        public const int WM_ACTIVATE = 0x0006;
        public const int WM_CHAR = 0x0102;
        public const int WM_CHAR2 = 0x0106;
        public const int WM_CLOSE = 0x0010;
        public const int WM_COMMAND = 0x0111;
        public const int WM_DEADCHAR = 0x0107;
        public const int WM_DESTROY = 0x0002;
        public const int WM_GETTEXT = 0x0D;
        public const int WM_GETTEXTLENGTH = 0x0E;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYDOWN2 = 0x0104;
        public const int WM_KEYFIRST = 0x0100;
        public const int WM_KEYLAST = 0x0108;
        public const int WM_KEYUP = 0x0101;
        public const int WM_KEYUP2 = 0x0105;
        public const int WM_LBUTTONDBLCLK = 0x0203;
        public const int WM_LBUTTONDBLCLK2 = 0x203;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_MOUSEFIRST = 0x0200;
        public const int WM_MOUSEHWHEEL = 0x020E;
        public const int WM_MOUSELAST = 0x020E;
        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_MOUSEWHEEL = 0x020A;
        public const int WM_RBUTTONDBLCLK = 0x0206;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_SETFOCUS = 0x0007;
        public const uint WM_SETTEXT = 0x000C;
        public const int WM_SYSCHAR = 0x0106;
        public const int WM_SYSCHAR2 = 0x0106;
        public const int WM_SYSDEADCHAR = 0x0107;
        public const int WM_SYSKEYDOWN = 0x0104;
        public const int WM_SYSKEYDOWN2 = 0x0104;
        public const int WM_SYSKEYFIRST = 0x0104;
        public const int WM_SYSKEYLAST = 0x0107;
        public const int WM_SYSKEYUP = 0x0105;
        public const int WM_SYSKEYUP2 = 0x0105;

        public const int VK_DOWN = 0x28;
        public const int VK_RETURN = 0x0D;
        public const int VK_TAB = 0x09;
        public const int VK_UP = 0x26;

        public const int LVIF_TEXT = 0x0001;
        public const int LVIF_IMAGE = 0x0002;
        public const int LVIF_PARAM = 0x0004;
        public const int LVIF_STATE = 0x0008;
        public const int LVIF_INDENT = 0x0010;
        public const int LVIF_NORECOMPUTE = 0x0800;
        public const int LVIF_GROUPID = 0x0100;
        public const int LVIF_COLUMNS = 0x0200;
        public const int LVIF_COLFMT = 0x0400;

        public const int LB_GETCURSEL = 0x0188;
        public const int LB_GETTEXT = 0x0189;
        public const int LB_GETTEXTLEN = 0x018A;
        public const int LB_GETCOUNT = 0x018B;
        public const int LB_SETCURSEL = 0x0186;
        public const int LB_GETSEL = 0x0187;
        public const int LB_GETSELITEMS = 0x0191;
        public const int LB_GETITEMDATA = 0x0199;
        public const int LB_SETITEMDATA = 0x019A;

        public const int LVM_GETITEM = 0x1005;
        public const int LVM_SETITEM = 0x1006;
        public const int LVM_GETITEMCOUNT = 0x1004;
        public const int LVM_GETITEMTEXT = 0x1073;

        public const int CB_GETCURSEL = 0x0147;
        public const int CB_GETLBTEXT = 0x0148;
        public const int CB_GETCOUNT = 0x0146;
        public const int CB_SELECTSTRING = 0x014D;
        public const int CB_SETCURSEL = 0x014E;

        public const int SW_HIDE = 0;
        public const int SW_SHOW = 1;
        public const int SW_RESTORE = 9;
        public const int SW_APPEND = 10;

        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;

        public const int WS_CHILD = 0x40000000;
        public const int WS_CLIPCHILDREN = 0x02000000;
        public const int WS_CLIPSIBLINGS = 0x04000000;
        public const int WS_DISABLED = 0x08000000;
        public const int WS_GROUP = 0x00020000;
        public const int WS_HSCROLL = 0x00100000;
        public const int WS_MAXIMIZE = 0x01000000;
        public const int WS_MAXIMIZEBOX = 0x00010000;
        public const int WS_MINIMIZE = 0x20000000;
        public const int WS_MINIMIZEBOX = 0x00020000;
        public const int WS_OVERLAPPED = 0x00000000;
        public const int WS_POPUP = unchecked((int)0x80000000);
        public const int WS_SIZEBOX = 0x00040000;
        public const int WS_SYSMENU = 0x00080000;
        public const int WS_TABSTOP = 0x00010000;
        public const int WS_THICKFRAME = 0x00040000;
        public const int WS_TILED = 0x00000000;
        public const int WS_VISIBLE = 0x10000000;
        public const int WS_BORDER = 0x00800000;
        public const int WS_EX_CLIENTEDGE = 0x00000200;
    }
}
