using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml;
using System.Collections;

namespace ScoreC
{
    class ProjectData
    {
        public static ProjectData Parse(string projectFile)
        {
            var result = new ProjectData();

            var document = new XmlDocument();
            document.LoadXml(File.ReadAllText(projectFile));

            var projectNodeList = document.GetElementsByTagName("project");
            if (projectNodeList.Count == 0)
            {
                Console.WriteLine("Malformed .sproj file: Project file must contain a <project> tag.");
                return null;
            }
            else if (projectNodeList.Count != 1)
            {
                Console.WriteLine("Malformed .sproj file: Project file contains too many <project> tags.");
                return null;
            }

            var projectData = projectNodeList[0] as XmlElement;
            result.Name = projectData.GetAttribute("name");

            var includes = new List<XmlElement>();
            var includeNodeList = projectData.GetElementsByTagName("include");

            foreach (var includeNodeObj in includeNodeList)
            {
                var includeNode = includeNodeObj as XmlElement;
                foreach (var includeChildNodeObj in includeNode.ChildNodes)
                {
                    var includeChildNode = includeChildNodeObj as XmlElement;
                    if (includeChildNode == null || includeChildNode.Name != "file")
                    {
                        Console.WriteLine("Malformed .sproj file: Include lists may only contain <file> elements.");
                        return null;
                    }
                    includes.Add(includeChildNode);
                }
            }

            foreach (var filePathNode in includes)
            {
                if (filePathNode.ChildNodes.Count != 0)
                {
                    Console.WriteLine("Malformed .sproj file: <file> elements cannot contain elements.");
                    return null;
                }

                var filePath = filePathNode.GetAttribute("name");
                if (filePath == null)
                {
                    Console.WriteLine("Malformed .sproj file: <file> elements must contain a `name` attribute with the relative file path.");
                    return null;
                }

                // FIXME(kai): Check that this is actually a valid path.
                result.IncludedFiles.Add(filePath);
            }
            
            return result;
        }

        public static void WriteToFile(ProjectData projectData, string projectDir)
        {
            var document = new XmlDocument();

            var projectElement = document.AppendChild(document.CreateElement("project")) as XmlElement;
            projectElement.SetAttribute("name", projectData.Name);
            projectElement.SetAttribute("source_dir", projectData.SourceDir);

            if (projectData.IncludedFiles.Count > 0)
            {
                var includeElement = projectElement.AppendChild(document.CreateElement("include")) as XmlElement;
                foreach (var include in projectData.IncludedFiles)
                {
                    var fileElement = includeElement.AppendChild(document.CreateElement("file")) as XmlElement;
                    fileElement.SetAttribute("name", include);
                }
            }

            document.Save(Path.Combine(projectDir, ".sproj"));
        }

        public string Name = "score_project";
        public string SourceDir = "src";

        // Files are relative to ./`SourceDir`/

        public List<string> IncludedFiles = new List<string>();
    }
}
