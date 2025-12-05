using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TeamCashCenter.Data;
using TeamCashCenter.Data.Model;

namespace TeamCashCenter.Services
{
    public class TeamService
    {
        private readonly CashCenterContext _db;
        public TeamService(CashCenterContext db)
        {
            _db = db;
        }

        public async Task<List<Team>> GetTeamsAsync()
        {
            return await _db.Set<Team>().OrderBy(t => t.Name).ToListAsync();
        }

        public async Task<Team?> GetTeamAsync(Guid id)
        {
            return await _db.Set<Team>().FindAsync(id);
        }

        public async Task<Team> AddTeamAsync(string name)
        {
            var team = new Team { Id = Guid.NewGuid(), Name = name };
            _db.Set<Team>().Add(team);
            await _db.SaveChangesAsync();
            return team;
        }

        public async Task<bool> UpdateTeamAsync(Guid id, string name)
        {
            var team = await _db.Set<Team>().FindAsync(id);
            if (team == null) return false;
            team.Name = name;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTeamAsync(Guid id)
        {
            var team = await _db.Set<Team>().FindAsync(id);
            if (team == null) return false;
            _db.Set<Team>().Remove(team);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
