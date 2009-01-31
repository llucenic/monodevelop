//
// StandardHeaderService.cs
//
// Author:
//   Mike Krüger <mkrueger@novell.com>
//   Michael Hutchinson <mhutchinson@novell.com>
//
// Copyright (C) 2007, 2009 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using MonoDevelop.Core;
using MonoDevelop.Projects;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.Ide.StandardHeader
{
	public static class StandardHeaderService
	{
	
		static string GetComment (string language)
		{
			LanguageBindingService languageBindingService = MonoDevelop.Projects.Services.Languages;
			ILanguageBinding binding = languageBindingService.GetBindingPerLanguageName (language);
			if (binding != null)
				return binding.CommentTag;
			return null;
		}
		
		public static string GetHeader (Project parentProject, string language, string fileName, bool newFile)
		{
			string comment = GetComment (language);
			if (comment == null)
				return "";
			
			StandardHeaderPolicy policy = parentProject != null
				? parentProject.Policies.Get<StandardHeaderPolicy> ()
				: MonoDevelop.Projects.Policies.PolicyService.GetDefaultPolicy<StandardHeaderPolicy> ();
				
			if (string.IsNullOrEmpty (policy.Text) || (newFile && !policy.IncludeInNewFiles))
				return "";
			
			AuthorInformation authorInfo = IdeApp.Workspace.GetAuthorInformation (parentProject);
			
			StringBuilder result = new StringBuilder (policy.Text.Length);
			string[] lines = policy.Text.Split ('\n');
			foreach (string line in lines) {
				result.Append (comment);
				result.Append (line);
				// the text editor should take care of conversions to preferred newline char
				result.Append ('\n');
			}
			
			return StringParserService.Parse (result.ToString(), new string[,] { 
				{ "FileName", Path.GetFileName (fileName) }, 
				{ "FileNameWithoutExtension", Path.GetFileNameWithoutExtension (fileName) }, 
				{ "Directory", Path.GetDirectoryName (fileName) }, 
				{ "FullFileName", fileName },
				{ "AuthorName", authorInfo.Name },
				{ "AuthorEmail", authorInfo.Email },
				{ "CopyrightHolder", authorInfo.Copyright },
				{ "ProjectName", parentProject == null? "" : parentProject.Name }
			});
		}
	}
}
