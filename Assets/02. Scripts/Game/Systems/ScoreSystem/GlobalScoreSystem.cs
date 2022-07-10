using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public interface IGlobalScoreSystem : ISystem {
        int ScorePerEffectiveDamage { get; }
        int ScorePerTransactionMoney { get; }
        int ScorePerEffectiveKill { get; }
        int ScorePerBountyFinished { get; }

        int ScorePerMissionFinished { get; }
        
        float WinningTeamScoreMultiplier { get; }
    }
    public class GlobalScoreSystem : AbstractNetworkedSystem, IGlobalScoreSystem {
        [field: SerializeField] public int ScorePerEffectiveDamage { get; private set; } = 1;
        [field: SerializeField] public int ScorePerTransactionMoney { get; private set; } = 2;
        [field: SerializeField] public int ScorePerEffectiveKill { get; private set; } = 30;
        [field: SerializeField] public int ScorePerBountyFinished { get; private set; } = 100;
        [field: SerializeField] public int ScorePerMissionFinished { get; private set; } = 100;
        [field: SerializeField] public float WinningTeamScoreMultiplier { get; private set; } = 1.5f;

        private void Awake() {
            Mikrocosmos.Interface.RegisterSystem<IGlobalScoreSystem>(this);
        }
    }
}
