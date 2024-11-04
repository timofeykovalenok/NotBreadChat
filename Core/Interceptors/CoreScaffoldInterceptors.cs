using LinqToDB.CodeModel;
using LinqToDB.DataModel;
using LinqToDB.Scaffold;
using LinqToDB.Schema;
using System.Diagnostics;

namespace Core.Interceptors
{
    public class CoreScaffoldInterceptors : ScaffoldInterceptors
    {
        private const string ClassNameSeparator = "In";

        public override IEnumerable<Table> GetTables(IEnumerable<Table> tables) =>
            base.GetTables(tables).Where(x => x.Name.Name != Constants.VersionTableName);

        public override void PreprocessEntity(ITypeParser typeParser, EntityModel entityModel)
        {
            base.PreprocessEntity(typeParser, entityModel);

            var substrings = entityModel.Class.Name.Split(ClassNameSeparator);
            entityModel.Class.Name = string.Join(ClassNameSeparator, substrings.Select(x => x.TrimEnd('s')));
        }

        public override void PreprocessAssociation(ITypeParser typeParser, AssociationModel associationModel)
        {
            base.PreprocessAssociation(typeParser, associationModel);

            if (associationModel.FromColumns?.Length == 1)
            {
                var fromColumnName = associationModel.FromColumns.First().Property.Name;

                if (fromColumnName.EndsWith("Id"))
                    associationModel.Property!.Name = fromColumnName[..^2];
                else
                    associationModel.Property!.Name = fromColumnName;
            }
            else
                associationModel.Property!.Name = associationModel.Source.Class.Name;

            if (associationModel.ManyToOne)
            {
                var backPropertyName = associationModel.BackreferenceProperty!.Name;
                associationModel.BackreferenceProperty!.Name = associationModel.Property.Name + backPropertyName;
            }
            else
                associationModel.BackreferenceProperty!.Name = associationModel.Target.Class.Name;
        }
    }
}
