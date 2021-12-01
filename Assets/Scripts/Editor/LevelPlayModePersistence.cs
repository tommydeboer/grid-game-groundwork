using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Editor
{
    public static class LevelPlayModePersistence
    {
        static string textFilePath => Application.persistentDataPath + "/playmodechanges.txt";

        public struct Job
        {
            public string name;
            public Vector3 position;
            public Vector3 eulerAngles;
        }

        public static IEnumerable<Job> GetJobs()
        {
            var jobs = new List<Job>();
            if (File.Exists(textFilePath))
            {
                string[] lines = File.ReadAllLines(textFilePath);
                foreach (string line in lines)
                {
                    string[] split = line.Split('|');
                    Job newJob = new Job();
                    if (line.Contains("clear"))
                    {
                        newJob.name = split[0];
                        newJob.position = Vec3FromStrings(split[1], split[2], split[3]);
                    }
                    else if (line.Contains("newobject"))
                    {
                        newJob.name = split[1];
                        newJob.position = Vec3FromStrings(split[2], split[3], split[4]);
                        newJob.eulerAngles = Vec3FromStrings(split[5], split[6], split[7]);
                    }

                    jobs.Add(newJob);
                }

                File.Delete(textFilePath);
            }

            return jobs.ToArray();
        }

        static Vector3 Vec3FromStrings(string stringX, string stringY, string stringZ)
        {
            if (int.TryParse(stringX, out int x))
            {
                if (int.TryParse(stringY, out int y))
                {
                    if (int.TryParse(stringZ, out int z))
                    {
                        return new Vector3(x, y, z);
                    }
                }
            }

            return Vector3.zero;
        }

        public static void SaveNewObject(GameObject go)
        {
            Vector3Int p = Utils.Vec3ToInt(go.transform.position);
            Vector3Int r = Utils.Vec3ToInt(go.transform.eulerAngles);
            string s = "newobject|" + go.transform.name + "|" + p.x + "|" + p.y + "|" + p.z + "|" + r.x + "|" + r.y +
                       "|" +
                       r.z;
            WriteText(s);
        }

        static void WriteText(string newText)
        {
            var lines = new List<string> {newText};
            File.AppendAllLines(textFilePath, lines);
        }
    }
}