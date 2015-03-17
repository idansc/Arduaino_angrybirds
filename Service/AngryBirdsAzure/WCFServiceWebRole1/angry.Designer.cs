﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

[assembly: EdmSchemaAttribute()]
namespace WCFServiceWebRole1
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class angrydbEntities1 : ObjectContext
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new angrydbEntities1 object using the connection string found in the 'angrydbEntities1' section of the application configuration file.
        /// </summary>
        public angrydbEntities1() : base("name=angrydbEntities1", "angrydbEntities1")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new angrydbEntities1 object.
        /// </summary>
        public angrydbEntities1(string connectionString) : base(connectionString, "angrydbEntities1")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new angrydbEntities1 object.
        /// </summary>
        public angrydbEntities1(EntityConnection connection) : base(connection, "angrydbEntities1")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        #endregion
    
        #region Partial Methods
    
        partial void OnContextCreated();
    
        #endregion
    
        #region ObjectSet Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<HighScore> HighScores
        {
            get
            {
                if ((_HighScores == null))
                {
                    _HighScores = base.CreateObjectSet<HighScore>("HighScores");
                }
                return _HighScores;
            }
        }
        private ObjectSet<HighScore> _HighScores;

        #endregion

        #region AddTo Methods
    
        /// <summary>
        /// Deprecated Method for adding a new object to the HighScores EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToHighScores(HighScore highScore)
        {
            base.AddObject("HighScores", highScore);
        }

        #endregion

    }

    #endregion

    #region Entities
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="angrydbModel", Name="HighScore")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class HighScore : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new HighScore object.
        /// </summary>
        /// <param name="id">Initial value of the ID property.</param>
        /// <param name="score">Initial value of the Score property.</param>
        public static HighScore CreateHighScore(global::System.Int32 id, global::System.Int32 score)
        {
            HighScore highScore = new HighScore();
            highScore.ID = id;
            highScore.Score = score;
            return highScore;
        }

        #endregion

        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ID
        {
            get
            {
                return _ID;
            }
            set
            {
                if (_ID != value)
                {
                    OnIDChanging(value);
                    ReportPropertyChanging("ID");
                    _ID = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("ID");
                    OnIDChanged();
                }
            }
        }
        private global::System.Int32 _ID;
        partial void OnIDChanging(global::System.Int32 value);
        partial void OnIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 Score
        {
            get
            {
                return _Score;
            }
            set
            {
                OnScoreChanging(value);
                ReportPropertyChanging("Score");
                _Score = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("Score");
                OnScoreChanged();
            }
        }
        private global::System.Int32 _Score;
        partial void OnScoreChanging(global::System.Int32 value);
        partial void OnScoreChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.DateTime> Time
        {
            get
            {
                return _Time;
            }
            set
            {
                OnTimeChanging(value);
                ReportPropertyChanging("Time");
                _Time = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("Time");
                OnTimeChanged();
            }
        }
        private Nullable<global::System.DateTime> _Time;
        partial void OnTimeChanging(Nullable<global::System.DateTime> value);
        partial void OnTimeChanged();

        #endregion

    
    }

    #endregion

    
}