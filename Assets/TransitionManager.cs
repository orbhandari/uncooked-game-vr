using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{

    public FadeScreen fadeScreen;

    public void GoToScene(int sceneIndex)
    {
        GoToSceneRoutine(sceneIndex);
    }

    IEnumerator GoToSceneRoutine(int sceneIndex)
    {
        fadeScreen.FadeOut();
        yield return new WaitForSeconds(fadeScreen.fadeDuration);

        SceneManager.LoadScene(sceneIndex);
    }

    public void GoToSceneAsync(int sceneIndex)
    {
        Debug.Log("Calling Go to Scene CoroutineAsync.");
        StartCoroutine(GoToSceneRoutineAsync(sceneIndex));
    }

    IEnumerator GoToSceneRoutineAsync(int sceneIndex)
    {
        Debug.Log("Fading out.");
        fadeScreen.FadeOut();

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        float timer = 0;
        while (timer <= fadeScreen.fadeDuration && !operation.isDone)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        operation.allowSceneActivation = true;
    }
}
