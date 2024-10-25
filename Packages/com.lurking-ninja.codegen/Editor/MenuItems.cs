/***
 * Game Foundation
 * Copyright (c) 2023-2024 Lurking Ninja.
 *
 * MIT License
 * https://github.com/LurkingNinja/com.lurking-ninja.game-foundation
 */
namespace LurkingNinja.CodeGen.Editor
{
	using UnityEditor;
	using UnityEngine;

	public static class MenuItems
    {
		private const string _MENU_BASE = "Tools/Lurking Ninja/CodeGen/";
		private const string _RUN_GENERATION = _MENU_BASE + "/Run Generation %&g";
		private const string _ABOUT = _MENU_BASE + "/About ";
		
		private const string _URL_TO_GITHUB = "https://github.com/LurkingNinja/com.lurking-ninja.codegen?tab=readme-ov-file#codegen-package";

		[MenuItem(_RUN_GENERATION, true, 10)]
		private static bool MenuRunGenerationValidator() => !CodeGenSettingsProvider.IsGenerating;

		[MenuItem(_RUN_GENERATION, false, 10)]
		internal static void MenuRunGeneration() => CodeGenSettingsProvider.RunGeneration();
	    
		[MenuItem(_ABOUT, false, 100)]
		private static void OpenReadme() => Application.OpenURL(_URL_TO_GITHUB);
    }
}