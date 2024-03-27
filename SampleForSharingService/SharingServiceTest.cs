using System.Threading.Tasks;
using RealityCollective.ServiceFramework.Services;
using SharingService;
using UnityEngine;

public class SharingServiceTest : MonoBehaviour
{
    [SerializeField]
    private GameObject sharingObjectPrefab;
    private async void Start()
    {
        if (ServiceManager.Instance.TryGetService<ISharingService>(out var sharingService))
        {
            await sharingService.CreateAndJoinRoom();
            Instantiate(sharingObjectPrefab);
            await Task.Delay(5000);
            await sharingService.LeaveRoom();
        }
    }
}
