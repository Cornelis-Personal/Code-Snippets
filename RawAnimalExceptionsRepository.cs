using System.Collections.Generic;
using System.Threading.Tasks;
using Marel.LairageScanner.Context.Sql.Contexts;
using Marel.LairageScanner.Context.Sql.TableRepositories.Interfaces;
using Marel.Models.LairageScanner.Data;
using Marel.Models.NLIS;
using System.Linq;

namespace Marel.LairageScanner.Context.Sql.TableRepositories.Repositories
{
    public class RawAnimalExceptionsRepository : GenericSQLRepository<RawAnimalExceptions>, IRawAnimalExceptionsRepository
    {
        public RawAnimalExceptionsRepository(NLISContext context) : base(context)
        { }

		// Join Example using Query Language
        public async Task<List<AnimalExceptionRecord>> GetAnimalExceptionsAsync(string rfid)
        {
            return (from animalException in Query()
                    where animalException.Rfidentity == rfid
                    join erpstatCodes in Query<Erpstatuscodes>() on animalException.ExceptionStatus equals erpstatCodes
                        .StatusCode
                    select new AnimalExceptionRecord()
                    {
                        Rfidentity = animalException.Rfidentity,
                        ProgramCode = animalException.ProgramCode,
                        ChangeDate = animalException.ChangeDate,
                        ExceptionStatus = erpstatCodes.StatusCode,
                        ExceptionShortMessage = erpstatCodes.ShortDescription,
                        ExceptionID = animalException.ExceptionID,
                        Nlisidentity = animalException.Nlisidentity,
                        Setting = animalException.Setting
                    }).ToList();
        }
    }
}