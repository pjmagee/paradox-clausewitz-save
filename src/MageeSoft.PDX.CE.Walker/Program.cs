// See https://aka.ms/new-console-template for more information

using MageeSoft.PDX.CE;


var path = "D:\\paradox-clausewitz-sav\\src\\MageeSoft.PDX.CE.Tests\\Stellaris\\TestData\\gamestate";
var root = PdxSaveReader.Read(File.ReadAllText(path: path));

Console.WriteLine(root.ToSaveString());