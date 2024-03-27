using System;
using SharingService;
using UnityEngine;

public class SharableStateTransform : MonoBehaviour
{
    private SharingServiceTransform _transform = SharingServiceTransform.Create();
    private SharingObjectBase sharingObject;
    private bool _pendingSendTransform = false;
    private Transform _sharedTransformSource = null;
    private Coroutine _sendingLiveMovements;
    
    // Start is called before the first frame update
    void Awake()
    {
        if (sharingObject == null)
        {
            sharingObject = GetComponentInChildren<SharingObjectBase>();
        }
        
    }

    private void Start()
    {
        if (sharingObject != null)
        {
            sharingObject.PropertyChanged += HandlePropertyChanged;
            sharingObject.TransformMessageReceived += HandleTransformMessage;
        }
    }

    private void Update()
    {
        SendTransform(true);
    }

    /// <summary>
    /// Notify the other users of the object's transform changes. This will send a one time event. If 'setProperty' is
    /// true, a property change will also be sent to the server.
    /// </summary>
    private void SendTransform(bool setProperty)
    {
        if (sharingObject != null && _sharedTransformSource != null)
        {
            _pendingSendTransform = false;            
            if (UpdateSharingServiceTransform())
            {
                sharingObject.SendTransformMessage(_transform);
            }

            if (setProperty)
            {
                sharingObject.SetProperty(SharableStrings.ObjectTransform, _transform);
            }
        }
        else
        {
            _pendingSendTransform = true;
        }
    }

    private void HandlePropertyChanged(ISharingServiceObject sender, string property, object input)
    {
        switch (input)
        {
            case SharingServiceTransform transform when property == SharableStrings.ObjectTransform:
                ReceiveTransform(transform);
                break;
        }
    }
    /// <summary>
    /// Handle transform messages received from the server. These are special messages that represent the object's
    /// transform.  
    /// </summary>
    private void HandleTransformMessage(SharingServiceTransform transform)
    {
        ReceiveTransform(transform);
    }
    
    /// <summary>
    /// Receive transform changes from the server. This transform will be applied to the object's movable transform.
    /// </summary>
    private void ReceiveTransform(SharingServiceTransform source)
    {
        if (_transform.Position == source.Position &&
            _transform.Rotation == source.Rotation &&
            _transform.Scale == source.Scale)
        {
            return;
        }

        _transform = source;
        UpdateSharedTransformSource();
    }
    
    /// <summary>
    /// Update the _sharedTransformSource's world pose, and local scale with the latest sharing transform
    /// </summary>
    private void UpdateSharedTransformSource()
    {
        if (_sharedTransformSource == null)
        {
            return;
        }
        
        _sharedTransformSource.localPosition = _transform.Position;
        _sharedTransformSource.localRotation = _transform.Rotation;
        _sharedTransformSource.localScale = _transform.Scale;
    }
    
    /// <summary>
    /// Update the sharing transform with _sharedTransformSource's world pose, and local scale
    /// </summary>
    private bool UpdateSharingServiceTransform()
    {
        if (_sharedTransformSource == null)
        {
            return false;
        }

        bool changed = _transform.SetScale(ref _sharedTransformSource);
        changed |= _transform.SetLocal(ref _sharedTransformSource);
        return changed;  
    }
    
}


