using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OnePF;

public class INAPP : MonoBehaviour {
	public static INAPP Instance;

//	private const int OFFSET = 5;
//	private const int BUTTON_WIDTH = 200;
//	private const int BUTTON_HEIGHT = 80;
//	
//	private const int SIDE_BUTTON_WIDTH = 140;
//	private const int SIDE_BUTTON_HEIGHT = 60;
//	
//	private const int WINDOW_WIDTH = 400;
//	private const int WINDOW_HEIGHT = 320;
//	
//	private const int FONT_SIZE = 24;
//	
//	private const int N_ROUNDS = 5;
//	
	public const string SKU_MEDKIT = "sku_medkit";
	public const string SKU_AMMO = "sku_ammo";
	public const string SKU_INFINITE_AMMO = "sku_infinite_ammo";
	public const string SKU_COWBOY_HAT = "sku_cowboy_hat";
	
	private bool _processingPayment = false;
	public static bool _purchaseDone = false;
	private bool _showShopWindow = false;
	private string _popupText = "";
	
	private GameObject[] _joysticks = null;
	

	private const string STORE_ONEPF = "org.onepf.store";
    const string SKU = "sku";
    const string SKU_Pack1 = "pack1";
    const string SKU_Pack2 = "pack2";
    const string SKU_Pack3 = "pack3";
    const string SKU_Pack4 = "pack4";

    string _label = "";
    bool _isInitialized = false;


	#region Billing
    private void Awake()
    {
        Instance = this;

        // Listen to all events for illustration purposes
        OpenIABEventManager.billingSupportedEvent += billingSupportedEvent;
        OpenIABEventManager.billingNotSupportedEvent += billingNotSupportedEvent;
        OpenIABEventManager.queryInventorySucceededEvent += queryInventorySucceededEvent;
        OpenIABEventManager.queryInventoryFailedEvent += queryInventoryFailedEvent;
        OpenIABEventManager.purchaseSucceededEvent += OnPurchaseSucceded;
        OpenIABEventManager.purchaseFailedEvent += purchaseFailedEvent;
        OpenIABEventManager.consumePurchaseSucceededEvent += consumePurchaseSucceededEvent;
        OpenIABEventManager.consumePurchaseFailedEvent += consumePurchaseFailedEvent;
        DontDestroyOnLoad( this );
    }
    private void OnDestroy()
    {
        // Remove all event handlers
        OpenIABEventManager.billingSupportedEvent -= billingSupportedEvent;
        OpenIABEventManager.billingNotSupportedEvent -= billingNotSupportedEvent;
        OpenIABEventManager.queryInventorySucceededEvent -= queryInventorySucceededEvent;
        OpenIABEventManager.queryInventoryFailedEvent -= queryInventoryFailedEvent;
        OpenIABEventManager.purchaseSucceededEvent -= OnPurchaseSucceded;
        OpenIABEventManager.purchaseFailedEvent -= purchaseFailedEvent;
        OpenIABEventManager.consumePurchaseSucceededEvent -= consumePurchaseSucceededEvent;
        OpenIABEventManager.consumePurchaseFailedEvent -= consumePurchaseFailedEvent;
    }

    private void Start()
    {
        // Map skus for different stores       
        OpenIAB.mapSku( SKU, OpenIAB_Android.STORE_GOOGLE, "sku" );
        OpenIAB.mapSku( SKU, OpenIAB_Android.STORE_AMAZON, "sku" );
        OpenIAB.mapSku( SKU, OpenIAB_iOS.STORE, "sku" );
    //    OpenIAB.mapSku( SKU, OpenIAB_WP8.STORE, "sku" );

        OpenIAB.mapSku( SKU_Pack1, OpenIAB_iOS.STORE, "pack1" );
        OpenIAB.mapSku( SKU_Pack2, OpenIAB_iOS.STORE, "pack2" );
        OpenIAB.mapSku( SKU_Pack3, OpenIAB_iOS.STORE, "pack3" );
        OpenIAB.mapSku( SKU_Pack4, OpenIAB_iOS.STORE, "pack4" );

        var publicKey = "------ Google Play public KEY here ----- ";

        var options = new Options();
        options.checkInventoryTimeoutMs = Options.INVENTORY_CHECK_TIMEOUT_MS * 2;
        options.discoveryTimeoutMs = Options.DISCOVER_TIMEOUT_MS * 2;
        options.checkInventory = false;
        options.verifyMode = OptionsVerifyMode.VERIFY_SKIP;
        options.prefferedStoreNames = new string[] { OpenIAB_Android.STORE_GOOGLE, OpenIAB_Android.STORE_AMAZON, OpenIAB_Android.STORE_YANDEX };
        options.storeKeys = new Dictionary<string, string> { { OpenIAB_Android.STORE_GOOGLE, publicKey } };

        // Transmit options and start the service
        OpenIAB.init( options );

    }

	
	// Verifies the developer payload of a purchase.
	bool VerifyDeveloperPayload(string developerPayload) {
		/*
         * TODO: verify that the developer payload of the purchase is correct. It will be
         * the same one that you sent when initiating the purchase.
         * 
         * WARNING: Locally generating a random string when starting a purchase and 
         * verifying it here might seem like a good approach, but this will fail in the 
         * case where the user purchases an item on one device and then uses your app on 
         * a different device, because on the other device you will not have access to the
         * random string you originally generated.
         *
         * So a good developer payload has these characteristics:
         * 
         * 1. If two different users purchase an item, the payload is different between them,
         *    so that one user's purchase can't be replayed to another user.
         * 
         * 2. The payload must be such that you can verify it even when the app wasn't the
         *    one who initiated the purchase flow (so that items purchased by the user on 
         *    one device work on other devices owned by the user).
         * 
         * Using your own server to store and verify developer payloads across app
         * installations is recommended.
         */
		return true;
	}
	
	private void OnBillingSupported() {
		Debug.Log("Billing is supported");
		Invoke("queryInventory",1);
	}

	void queryInventory(){
		OpenIAB.queryInventory();
	}
	
	private void OnBillingNotSupported(string error) {
//		Debug.Log("Billing not supported: " + error);
	}
	
	private void OnQueryInventorySucceeded(Inventory inventory) {
		Debug.Log("Query inventory succeeded: " + inventory);
		foreach (Purchase item in inventory.GetAllPurchases ()) {
			OpenIAB.consumeProduct(item);
		}
//		
//		// Do we have the infinite ammo subscription?
//		Purchase infiniteAmmoPurchase = inventory.GetPurchase(SKU_INFINITE_AMMO);
//		bool subscribedToInfiniteAmmo = (infiniteAmmoPurchase != null && VerifyDeveloperPayload(infiniteAmmoPurchase.DeveloperPayload));
//		Debug.Log("User " + (subscribedToInfiniteAmmo ? "HAS" : "DOES NOT HAVE") + " infinite ammo subscription.");
//		if (subscribedToInfiniteAmmo) {
//			_playerAmmoBox.IsInfinite = true;
//		}
//		
//		// Check cowboy hat purchase
//		Purchase cowboyHatPurchase = inventory.GetPurchase(SKU_COWBOY_HAT);
//		bool isCowboyHat = (cowboyHatPurchase != null && VerifyDeveloperPayload(cowboyHatPurchase.DeveloperPayload));
//		Debug.Log("User " + (isCowboyHat ? "HAS" : "HAS NO") + " cowboy hat");
//		_playerHat.PutOn = isCowboyHat;
//		
//		// Check for delivery of expandable items. If we own some, we should consume everything immediately
//		Purchase medKitPurchase = inventory.GetPurchase(SKU_MEDKIT);
//		if (medKitPurchase  != null && VerifyDeveloperPayload(medKitPurchase.DeveloperPayload)) {
//			//Debug.Log("We have MedKit. Consuming it.");
//			OpenIAB.consumeProduct(inventory.GetPurchase(SKU_MEDKIT));
//		}
//		Purchase ammoPurchase = inventory.GetPurchase(SKU_AMMO);
//		if (ammoPurchase != null && VerifyDeveloperPayload(ammoPurchase.DeveloperPayload)) {
//			//Debug.Log("We have ammo. Consuming it.");
//			OpenIAB.consumeProduct(inventory.GetPurchase(SKU_AMMO));
//		}
	}
	
	private void OnQueryInventoryFailed(string error) {
		Debug.Log("Query inventory failed: " + error);
	}
	
	private void OnPurchaseSucceded(Purchase purchase) {
		Debug.Log("Purchase succeded: " + purchase.Sku + "; Payload: " + purchase.DeveloperPayload);

		//MainMap.WasInApp = true;
		PlayerPrefs.SetInt("WasInApp", 1);
		PlayerPrefs.Save();
		_purchaseDone = true;
		InitScriptName.InitScript.Instance.PurchaseSucceded();
		OpenIAB.consumeProduct(purchase);
		if (!VerifyDeveloperPayload(purchase.DeveloperPayload)) {
			return;
		}
//		switch (purchase.Sku) {
//		case SKU_MEDKIT:
//			OpenIAB.consumeProduct(purchase);
//			return;
//		case SKU_AMMO:
//			OpenIAB.consumeProduct(purchase);
//			return;
//		case SKU_COWBOY_HAT:
//			_playerHat.PutOn = true;
//			break;
//		case SKU_INFINITE_AMMO:
//			_playerAmmoBox.IsInfinite = true;
//			break;
//		default:
//			Debug.LogWarning("Unknown SKU: " + purchase.Sku);
//			break;
//		}
		_processingPayment = false;
	}
	
	private void OnPurchaseFailed(string error) {
		Debug.Log("Purchase failed: " + error);
		_processingPayment = false;
	}
	
	private void OnConsumePurchaseSucceeded(Purchase purchase) {
		Debug.Log("Consume purchase succeded: " + purchase.ToString());
//		foreach (var item in MainMenu.pricing) {
//			if(purchase.AppstoreName.Contains("coins"))
//			{
//				Debug.Log(purchase.AppstoreName.Substring(purchase.AppstoreName.Length,1));
//				if(item.Get<string>("Good") == "Coins" + purchase.AppstoreName.Substring(purchase.AppstoreName.Length,1).ToString()) {
//					MainMenu.coins += item.Get<int>("Price");
//			}
//				if(purchase.AppstoreName.Contains("gems"))
//				{
//					if(item.Get<string>("Good") == "Gems" + purchase.AppstoreName.Substring(purchase.AppstoreName.Length,1).ToString()) {
//						MainMenu.coins += item.Get<int>("Price");
//					}
//				}
//			}
//		}
	}
	
	private void OnConsumePurchaseFailed(string error) {
		Debug.Log("Consume purchase failed: " + error);
		_processingPayment = false;
	}
	
	private void OnTransactionRestored(string sku) {
		Debug.Log("Transaction restored: " + sku);
	}
	
	private void OnRestoreSucceeded() {
		Debug.Log("Transactions restored successfully");
	}
	
	private void OnRestoreFailed(string error) {
		Debug.Log("Transaction restore failed: " + error);
	}
	#endregion // Billing
	
	#region GUI
	public void purchaseProduct(string good){
		_purchaseDone = false;
			OpenIAB.purchaseProduct(good);

	}

	#endregion // GUI

    private void billingSupportedEvent()
    {
        _isInitialized = true;
        Debug.Log( "billingSupportedEvent" );
    }
    private void billingNotSupportedEvent( string error )
    {
        Debug.Log( "billingNotSupportedEvent: " + error );
    }
    private void queryInventorySucceededEvent( Inventory inventory )
    {
        Debug.Log( "queryInventorySucceededEvent: " + inventory );
        if( inventory != null )
            _label = inventory.ToString();
    }
    private void queryInventoryFailedEvent( string error )
    {
        Debug.Log( "queryInventoryFailedEvent: " + error );
        _label = error;
    }
    private void purchaseSucceededEvent( Purchase purchase )
    {
        Debug.Log( "purchaseSucceededEvent: " + purchase );
        _label = "PURCHASED:" + purchase.ToString();
    }
    private void purchaseFailedEvent( int errorCode, string errorMessage )
    {
        Debug.Log( "purchaseFailedEvent: " + errorMessage );
        _label = "Purchase Failed: " + errorMessage;
    }
    private void consumePurchaseSucceededEvent( Purchase purchase )
    {
        Debug.Log( "consumePurchaseSucceededEvent: " + purchase );
        _label = "CONSUMED: " + purchase.ToString();
    }
    private void consumePurchaseFailedEvent( string error )
    {
        Debug.Log( "consumePurchaseFailedEvent: " + error );
        _label = "Consume Failed: " + error;
    }
}


