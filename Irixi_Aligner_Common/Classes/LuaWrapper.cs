﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.Ioc;

namespace Irixi_Aligner_Common.Classes
{
    public class LuaWrapper
    {
        private ScriptHelpMgr Scriptmgr = null;
        public NLua.Lua lua=null;
        public LuaWrapper()
        {
            try
            {
                lua = new NLua.Lua();
                lua.SetDebugHook(NLua.Event.EventMasks.LUA_MASKLINE, 1);
                lua.LoadCLRPackage();
                Scriptmgr = ScriptHelpMgr.Instance;
                RegistAllFunction();
            }
            catch(Exception ex)
            {
                throw new Exception(string.Format("Lua解释器创建失败: {0}",ex.Message));
            }
        }
        ~LuaWrapper()
        {
            //lua.RemoveDebugHook();
        }
        public void RegisterLuaFunc(NLua.Lua lua,string luaName,object target,string rawFunc)
        {
            lua.RegisterFunction(luaName, target, target.GetType().GetMethod(rawFunc));
        }
        private void RegistAllFunction()
        {
            //Register function
            Type type = Scriptmgr.GetType();
            System.Reflection.MethodInfo[] ps = type.GetMethods();
            List<MethodInfo> LuaFuncs = (from mi in ps where mi.Name.StartsWith("LuaF") select mi).ToList();
            foreach (var func in LuaFuncs)
            {
                string strLuaFunc = func.Name.Replace("LuaF_", "");
                RegisterLuaFunc(lua, strLuaFunc, Scriptmgr, func.Name); 
            }

        }
        public void DoString(string str)
        {
            byte[] bs = Encoding.UTF8.GetBytes(str);
            lua.DoString(bs); 
        }
        public void DoFile(string fileName)
        {
            lua.DoFile(fileName);
        }
        public void LoadString(string str)
        {
            lua.LoadString(str, "Check");
        }
        public void CloseLua()
        {
            lua.Close();
        }
    }
}
