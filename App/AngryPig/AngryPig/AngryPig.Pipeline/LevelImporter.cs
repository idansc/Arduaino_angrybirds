using System;
using System.Linq;
using System.Xaml;
using AngryPig.Data;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace AngryPig.Pipeline
{
    [ContentImporter(".xaml", DisplayName = "AngryPig Level Importer")]
    public class LevelImporter : ContentImporter<LevelInfo>
    {
        public override LevelInfo Import(string filename, ContentImporterContext context)
        {
            var level = new LevelInfo();
            var canvas = (Canvas)XamlServices.Load(filename);
            return level;
        }
    }
}