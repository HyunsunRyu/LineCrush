using System;
using UnityEngine;
using UnityEngine.Purchasing;

public class InAppPurchaser : MonoBehaviour, IStoreListener
{
    private static InAppPurchaser instance = null;
    private static IStoreController storeController;
    private static IExtensionProvider extensionProvider;

    private static InAppPurchaser GetInstance()
    {
        if (instance == null)
            instance = FindObjectOfType<InAppPurchaser>();
        if (instance == null)
        {
            GameObject obj = new GameObject("InAppPurchaser");
            instance = obj.AddComponent<InAppPurchaser>();
            instance.InitializePurchasing();
        }
        return instance;
    }

    private Action<string> purchaseCallback = null;
    
    private void Awake()
    {
        GetInstance().InitializePurchasing();
    }

    private bool IsInitialized()
    {
        return (storeController != null && extensionProvider != null);
    }

    public static void Initialize()
    {
        GetInstance().InitializePurchasing();
    }

    private void InitializePurchasing()
    {
        if (IsInitialized())
            return;

        try
        {
            var module = StandardPurchasingModule.Instance();

            ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);

            for (int i = 0, max = DataManager.GetInstance().GetProductListCount(); i < max; i++)
            {
                string productKey = "";
                if (DataManager.GetInstance().TryGetProductKey(i, ref productKey))
                {
                    builder.AddProduct(productKey, ProductType.Consumable, new IDs
                {
                    { productKey, AppleAppStore.Name },
                    { productKey, GooglePlay.Name },
                });
                }
            }
            UnityPurchasing.Initialize(this, builder);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    public static void BuyProduct(string productId, Action<string> callback)
    {
        GetInstance().BuyProductID(productId, callback);
    }

    private void BuyProductID(string productId, Action<string> callback)
    {
        try
        {
            if (IsInitialized())
            {
                Product p = storeController.products.WithID(productId);

                if (p != null && p.availableToPurchase)
                {
                    purchaseCallback = callback;
                    storeController.InitiatePurchase(p);
                }
                else
                {
                    if (callback != null)
                        callback("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else
            {
                if (callback != null)
                    callback("BuyProductID FAIL. Not initialized.");
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }
        catch (Exception e)
        {
            if (callback != null)
                callback("BuyProductID: FAIL. Exception during purchase. \n" + e);
            Debug.Log("BuyProductID: FAIL. Exception during purchase. \n" + e);
        }
    }

    public static void RestorePurchase()
    {
        GetInstance().RestorePurchaseFunc();
    }

    private void RestorePurchaseFunc()
    {
        if (!IsInitialized())
        {
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log("RestorePurchases started ...");

            var apple = extensionProvider.GetExtension<IAppleExtensions>();

            apple.RestoreTransactions((result) =>
            {
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            });
        }
        else
        {
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }

    public void OnInitialized(IStoreController sc, IExtensionProvider ep)
    {
        Debug.Log("OnInitialized : PASS");

        storeController = sc;
        extensionProvider = ep;
    }

    public void OnInitializeFailed(InitializationFailureReason reason)
    {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + reason);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (purchaseCallback != null)
            purchaseCallback(args.purchasedProduct.definition.id);
        
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        if (purchaseCallback != null)
            purchaseCallback("OnPurchaseFailed");
    }
}