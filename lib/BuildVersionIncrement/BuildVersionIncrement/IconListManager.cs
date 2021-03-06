using System;
using System.Collections;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

// Copied from codeproject article by Paul Inges
// Obtaining (and managing) file and folder icons using SHGetFileInfo in C#

namespace Etier.IconHelper
{
    /// <summary>
    /// Maintains a list of currently added file extensions
    /// </summary>
    public class IconListManager
    {
        private Hashtable _extensionList = new Hashtable();
        private System.Collections.ArrayList _imageLists = new ArrayList();			//will hold ImageList objects
        private IconSize _iconSize;
        private bool _manageBothSizes; //flag, used to determine whether to create two ImageLists.

        /// <summary>
        /// Creates an instance of <c>IconListManager</c> that will add icons to a single <c>ImageList</c> using the
        /// specified <c>IconSize</c>.
        /// </summary>
        /// <param name="imageList"><c>ImageList</c> to add icons to.</param>
        /// <param name="iconSize">Size to use (either 32 or 16 pixels).</param>
        public IconListManager(System.Windows.Forms.ImageList imageList, IconSize iconSize)
        {
            // Initialise the members of the class that will hold the image list we're
            // targeting, as well as the icon size (32 or 16)
            _imageLists.Add(imageList);
            _iconSize = iconSize;
        }

        /// <summary>
        /// Creates an instance of IconListManager that will add icons to two <c>ImageList</c> types. The two
        /// image lists are intended to be one for large icons, and the other for small icons.
        /// </summary>
        /// <param name="smallImageList">The <c>ImageList</c> that will hold small icons.</param>
        /// <param name="largeImageList">The <c>ImageList</c> that will hold large icons.</param>
        public IconListManager(System.Windows.Forms.ImageList smallImageList, System.Windows.Forms.ImageList largeImageList)
        {
            //add both our image lists
            _imageLists.Add(smallImageList);
            _imageLists.Add(largeImageList);

            //set flag
            _manageBothSizes = true;
        }

        /// <summary>
        /// Used internally, adds the extension to the hashtable, so that its value can then be returned.
        /// </summary>
        /// <param name="Extension"><c>String</c> of the file's extension.</param>
        /// <param name="ImageListPosition">Position of the extension in the <c>ImageList</c>.</param>
        private void AddExtension(string Extension, int ImageListPosition)
        {
            _extensionList.Add(Extension, ImageListPosition);
        }

        /// <summary>
        /// Called publicly to add a file's icon to the ImageList.
        /// </summary>
        /// <param name="filePath">Full path to the file.</param>
        /// <returns>Integer of the icon's position in the ImageList</returns>
        public int AddFileIcon(string filePath)
        {
            // Check if the file exists, otherwise, throw exception.
            //if (!System.IO.File.Exists( filePath )) throw new System.IO.FileNotFoundException("File does not exist");

            // Split it down so we can get the extension
            //string[] splitPath = filePath.Split(new Char[] { '.' });
            string extension = Path.GetExtension(filePath).TrimStart(".".ToCharArray()); // (string)splitPath.GetValue(splitPath.GetUpperBound(0));

            //Check that we haven't already got the extension, if we have, then
            //return back its index
            if (_extensionList.ContainsKey(extension.ToUpper()))
            {
                return (int)_extensionList[extension.ToUpper()];		//return existing index
            }
            else
            {
                // It's not already been added, so add it and record its position.

                int pos = ((ImageList)_imageLists[0]).Images.Count;		//store current count -- new item's index

                if (_manageBothSizes == true)
                {
                    //managing two lists, so add it to small first, then large
                    ((ImageList)_imageLists[0]).Images.Add(IconReader.GetFileIcon(filePath, IconSize.Small, false));
                    ((ImageList)_imageLists[1]).Images.Add(IconReader.GetFileIcon(filePath, IconSize.Large, false));
                }
                else
                {
                    //only doing one size, so use IconSize as specified in _iconSize.
                    ((ImageList)_imageLists[0]).Images.Add(IconReader.GetFileIcon(filePath, _iconSize, false));	//add to image list
                }

                AddExtension(extension.ToUpper(), pos);	// add to hash table
                return pos;
            }
        }


        /// <summary>
        /// Adds a folder iconto the imagelist.
        /// </summary>
        /// <param name="type">The type of folder (opened/closed).</param>
        /// <returns>Index in the imagelist pointing to the icon.</returns>
        public int AddFolderIcon(FolderType type)
        {
            string key = "folder_" + type.ToString();

            if (_extensionList.ContainsKey(key))
                return (int)_extensionList[key];
            else
            {
                // It's not already been added, so add it and record its position.

                int pos = ((ImageList)_imageLists[0]).Images.Count;		//store current count -- new item's index

                if (_manageBothSizes == true)
                {
                    //managing two lists, so add it to small first, then large
                    ((ImageList)_imageLists[0]).Images.Add(IconReader.GetFolderIcon(IconSize.Small, type));
                    ((ImageList)_imageLists[1]).Images.Add(IconReader.GetFolderIcon(IconSize.Large, type));
                }
                else
                {
                    //only doing one size, so use IconSize as specified in _iconSize.
                    ((ImageList)_imageLists[0]).Images.Add(IconReader.GetFolderIcon(_iconSize, type));	//add to image list
                }

                AddExtension(key, pos);	// add to hash table
                return pos;
            }
        }

        /// <summary>
        /// Clears any <c>ImageLists</c> that <c>IconListManager</c> is managing.
        /// </summary>
        public void ClearLists()
        {
            foreach (ImageList imageList in _imageLists)
            {
                imageList.Images.Clear();	//clear current imagelist.
            }

            _extensionList.Clear();			//empty hashtable of entries too.
        }
    }
}
