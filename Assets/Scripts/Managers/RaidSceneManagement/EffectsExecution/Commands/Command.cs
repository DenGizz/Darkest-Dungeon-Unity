using System;
using System.Collections;
using UnityEngine;

namespace Managers.RaidSceneManagement.EffectsExecution.Routines
{
    public class Command : ICommand
    {
        protected Coroutine  StartCoroutine(IEnumerator routine)
        {
            throw new NotImplementedException();
        }

        protected GameObject Instantiate(GameObject go)
        {
            throw new NotImplementedException();
        }
    }
}