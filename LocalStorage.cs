using Marel.MSAGradingAssesment.GunModels.Data.MSAGrading;
using Newtonsoft.Json;
using PCLStorage;
using System.Collections.Generic;
using System.Linq;

namespace Marel.MSAGradingAssesment.GunModels.Storage
{
	// This class is an Xamarin Forms Storage Management
    public static class LocalStorage
    {
        #region Private Constants

        private const string bodies = "Bodies.json";

        #endregion Private Constants

        #region Public Variables

        public static List<Body> Bodies { get; set; }

        /// <summary>
        /// Used in MSA Grading
        /// </summary>
        public static Body ActiveBody { get; set; }

        #endregion Public Variables

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        static LocalStorage()
        {
            // Initalize the Lists
            Bodies = new List<Body>();

            // Create local storage for MSA records
            InitBodies(bodies);
        }

        private static async void InitBodies(string fileName)
        {
            // Get the local storages
            var folder = FileSystem.Current.LocalStorage;
            var fileExist = await folder.CheckExistsAsync(fileName);

            // Create the file if it doesn't exists
            // else load from it
            if (fileExist == ExistenceCheckResult.FileExists)
            {
                var settings = await folder.GetFileAsync(fileName);
                var json = await settings.ReadAllTextAsync();

                // Read the settings file
                Bodies = JsonConvert.DeserializeObject<List<Body>>(json);
            }
            else
            {
                IFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                Bodies = new List<Body>();
                await file.WriteAllTextAsync(JsonConvert.SerializeObject(Bodies));
            }
        }

        #endregion Constructors

        #region Public Functions

        /// <summary>
        /// Clear all data
        /// </summary>
        public static void ClearAllData()
        {
            // Deal with active bodies first
            RemoveActive();
            ClearActive();

            // Clear both the lists
            Bodies.Clear();
        }

        /// <summary>
        /// Set the active data body
        /// </summary>
        /// <param name="body"></param>
        /// <param name="commit"></param>
        public static void SetActive(Body body, bool commit = false)
        {
            ActiveBody = null;
            ActiveBody = body;
        }

        /// <summary>
        /// Add a range of bodies.
        /// Commit it to storage if required.
        /// </summary>
        /// <param name="bodies"></param>
        /// <param name="commit"></param>
        public static void AddRange(List<Body> bodies, bool commit = false)
        {
            foreach (var body in bodies)
            {
                var doesExist = Bodies.FirstOrDefault(x => x.BodyNo == body.BodyNo && x.KillDate == body.KillDate) != null;
                if (doesExist)
                    continue;

                Bodies.Add(body);
            }

            if (commit) Commit();
        }

        /// <summary>
        /// Commit the data to file
        /// </summary>
        public static async void Commit()
        {
            // Save the bodies
            IFile bod = await FileSystem.Current.LocalStorage.CreateFileAsync(bodies, CreationCollisionOption.ReplaceExisting);
            await bod.WriteAllTextAsync(JsonConvert.SerializeObject(bodies));
        }

        /// <summary>
        /// Clear the active body
        /// </summary>
        public static void ClearActive()
        {
            ActiveBody = null;
        }

        /// <summary>
        /// Remove the active Body
        /// </summary>
        public static void RemoveActive()
        {
            Bodies.Remove(ActiveBody);
        }

        #endregion Public Functions
    }
}