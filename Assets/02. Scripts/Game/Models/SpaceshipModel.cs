using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using UnityEngine;

namespace Mikrocosmos
{
    public interface ISpaceshipModel : IModel {
        public BindableProperty<float> MoveForce { get; set; }
        public BindableProperty<float> MaxSpeed { get; set; }
    }
    public class SpaceshipModel : AbstractModel, ISpaceshipModel, ICanGetModel
    {
        protected override void OnInit() {
            MoveForce.Value = this.GetModel<SpaceshipConfigurationModel>().MoveForce;
            MaxSpeed.Value = this.GetModel<SpaceshipConfigurationModel>().MaxSpeed;
        }

        public BindableProperty<float> MoveForce { get; set; } = new BindableProperty<float>();
        public BindableProperty<float> MaxSpeed { get; set; } = new BindableProperty<float>();
    }
}
