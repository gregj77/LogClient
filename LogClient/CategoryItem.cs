using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace LogClient
{
    /// <summary>
    /// Category item.
    /// </summary>
    [Serializable]
    public sealed class CategoryItem : INotifyPropertyChanged, ISerializable
    {
        /// <summary>
        /// Category name.
        /// </summary>
        private string category;

        /// <summary>
        /// Category status: active or inactive.
        /// </summary>
        private bool active;

        /// <summary>
        /// Initializes a new instance of the CategoryItem class.
        /// </summary>
        public CategoryItem() : this(string.Empty, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CategoryItem class.
        /// </summary>
        /// <param name="category">Category name.</param>
        /// <param name="active">Category status.</param>
        public CategoryItem(string category, bool active)
        {
            this.category = category;
            this.active = active;
        }
        
        /// <summary>
        /// Initializes a new instance of the CategoryItem class.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        private CategoryItem(SerializationInfo info, StreamingContext context)
            : this(info.GetString("Category"), info.GetBoolean("IsActive"))
        {
        }

        /// <summary>
        /// Event to be triggered when property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets category name.
        /// </summary>
        public string Category 
        { 
            get { return this.category; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether category status.
        /// </summary>
        public bool Active 
        {
            get
            {
                return this.active;
            }

            set 
            { 
                this.active = value;
                if (null != this.PropertyChanged)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Active"));
                }
            }
        }

        /// <summary>
        /// Handles serialization of the object.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        [SecurityPermission(
            SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Category", this.category);
            info.AddValue("IsActive", this.active);
        }
    }
}
