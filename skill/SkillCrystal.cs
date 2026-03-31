using System;
using System.Collections;
using UnityEngine;

public class SkillCrystal : MonoBehaviour
{
    private void Awake()
    {
        EventBus.Instance.Subscribe(EventNames.OnMagic, (o) =>
        {
            if (o is ValueTuple<string, Transform> tuple)
            {
                var skillName = tuple.Item1;
                var skillCenterPos = tuple.Item2;

                var skillCfg = SkillManager.Instance.GetSkillByName(skillName);
                if (skillCfg == null)
                {
                    Debug.Log("skill cfg is null!");
                    return;
                }

                var skillObj = Instantiate(skillCfg.prefab, skillCenterPos.position, Quaternion.identity);
                var main = skillObj.GetComponent<ParticleSystem>().main;
                main.loop = false;

                StartCoroutine(WaitForRemoveTrigger(skillObj, main));

                main.stopAction = ParticleSystemStopAction.Destroy;
            }
        });
    }

    IEnumerator WaitForRemoveTrigger(GameObject skillObj, ParticleSystem.MainModule main)
    {
        yield return new WaitForSeconds(main.startLifetime.constant * 0.6f);

        skillObj.transform.Find("Crystals").GetComponent<SphereCollider>().enabled = false;
    }
}