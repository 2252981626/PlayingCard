package com.citydaheng.solitaire.ads;

import android.app.Activity;
import android.util.Log;

import com.applovin.mediation.MaxAd;
import com.applovin.mediation.MaxAdRevenueListener;
import com.applovin.mediation.MaxError;
import com.applovin.mediation.MaxReward;
import com.applovin.mediation.MaxRewardedAdListener;
import com.applovin.mediation.ads.MaxInterstitialAd;
import com.applovin.mediation.ads.MaxRewardedAd;
import com.applovin.sdk.AppLovinMediationProvider;
import com.applovin.sdk.AppLovinSdk;
import com.applovin.sdk.AppLovinSdkInitializationConfiguration;
import com.ctx.mod.Credential;
import com.ctx.mod.Message;
import com.ctx.mod.Queue;
import com.ctx.mod.TokenConfig;
import com.unity3d.player.UnityPlayer;

import org.json.JSONObject;

import java.util.ArrayDeque;

public final class NativeMaxBridge {
    private static final String TAG = "NativeMaxBridge";
    private static volatile Activity activity;
    private static volatile String receiverName = "";
    private static volatile boolean initialized;
    private static boolean initializing;
    private static String interstitialId = "";
    private static String rewardedId = "";
    private static MaxInterstitialAd interstitial;
    private static MaxRewardedAd rewarded;
    private static AdListener interstitialListener;
    private static AdListener rewardedListener;

    private NativeMaxBridge() {
    }

    public static void initialize(Activity unityActivity, String sdkKey,
                                  String interstitialAdUnitId, String rewardedAdUnitId,
                                  String sndoAppId, String unityReceiverName) {
        activity = unityActivity;
        receiverName = safe(unityReceiverName);
        runOnUi("initialization_failed", "", () -> {
            if (initialized || initializing) return;
            if (safe(sdkKey).isEmpty() || safe(interstitialAdUnitId).isEmpty() ||
                safe(rewardedAdUnitId).isEmpty() || receiverName.isEmpty()) {
                fail("initialization_failed", "", "Required MAX configuration is missing.");
                return;
            }

            initializing = true;
            interstitialId = interstitialAdUnitId;
            rewardedId = rewardedAdUnitId;
            SndoRun.configure(safe(sndoAppId));

            AppLovinSdk sdk = AppLovinSdk.getInstance(activity);
            if (sdk.isInitialized()) {
                finishInitialization();
                return;
            }

            AppLovinSdkInitializationConfiguration configuration =
                AppLovinSdkInitializationConfiguration.builder(sdkKey, activity)
                    .setMediationProvider(AppLovinMediationProvider.MAX)
                    .build();
            sdk.initialize(configuration, sdkConfiguration ->
                runOnUi("initialization_failed", "", NativeMaxBridge::finishInitialization));
        });
    }

    private static void finishInitialization() {
        if (initialized) return;

        if (interstitial != null) interstitial.destroy();
        if (rewarded != null) rewarded.destroy();

        interstitialListener = new AdListener(false);
        rewardedListener = new AdListener(true);
        interstitial = new MaxInterstitialAd(interstitialId, activity);
        interstitial.setListener(interstitialListener);
        interstitial.setRevenueListener(interstitialListener);
        rewarded = MaxRewardedAd.getInstance(rewardedId, activity);
        rewarded.setListener(rewardedListener);
        rewarded.setRevenueListener(rewardedListener);

        initializing = false;
        initialized = true;
        Log.i(TAG, "MAX initialized.");
        emit("initialized", "", null);
    }

    public static void loadInterstitial() {
        runOnUi("load_failed", "interstitial", () -> {
            if (!initialized || interstitial == null) {
                fail("load_failed", "interstitial", "MAX is not initialized.");
                return;
            }
            interstitialListener.ready = false;
            interstitial.loadAd();
        });
    }

    public static boolean isInterstitialReady() {
        return initialized && interstitialListener != null && interstitialListener.ready;
    }

    public static void showInterstitial() {
        runOnUi("display_failed", "interstitial", () -> {
            if (interstitialListener != null && interstitialListener.showing) {
                Log.w(TAG, "Ignored an interstitial request while it is showing.");
                return;
            }
            if (!isInterstitialReady() || rewardedListener.showing || !interstitial.isReady()) {
                fail("display_failed", "interstitial", "Interstitial is not ready.");
                return;
            }
            interstitialListener.ready = false;
            interstitialListener.showing = true;
            interstitial.showAd(activity);
        });
    }

    public static void loadRewarded() {
        runOnUi("load_failed", "rewarded", () -> {
            if (!initialized || rewarded == null) {
                fail("load_failed", "rewarded", "MAX is not initialized.");
                return;
            }
            rewardedListener.ready = false;
            rewarded.loadAd();
        });
    }

    public static boolean isRewardedReady() {
        return initialized && rewardedListener != null && rewardedListener.ready;
    }

    public static void showRewarded() {
        runOnUi("display_failed", "rewarded", () -> {
            if (rewardedListener != null && rewardedListener.showing) {
                Log.w(TAG, "Ignored a rewarded request while it is showing.");
                return;
            }
            if (!isRewardedReady() || interstitialListener.showing || !rewarded.isReady()) {
                fail("display_failed", "rewarded", "Rewarded ad is not ready.");
                return;
            }
            rewardedListener.ready = false;
            rewardedListener.showing = true;
            rewardedListener.rewardPending = true;
            rewarded.showAd(activity);
        });
    }

    private static void runOnUi(String failureEvent, String adType, Runnable action) {
        Activity current = activity;
        if (current == null) {
            fail(failureEvent, adType, "Unity activity is unavailable.");
            return;
        }
        try {
            current.runOnUiThread(() -> {
                try {
                    action.run();
                } catch (Exception | LinkageError error) {
                    fail(failureEvent, adType, "Native SDK call failed (" +
                         error.getClass().getSimpleName() + ").");
                }
            });
        } catch (Exception | LinkageError error) {
            fail(failureEvent, adType, "Android UI dispatch failed.");
        }
    }

    private static void fail(String event, String adType, String message) {
        if ("initialization_failed".equals(event)) initializing = false;
        AdListener listener = "rewarded".equals(adType) ? rewardedListener : interstitialListener;
        if (listener != null) {
            listener.ready = false;
            if ("display_failed".equals(event)) {
                listener.showing = false;
                listener.rewardPending = false;
            }
        }
        emit(event, adType, null, -1, message, null);
    }

    private static void emit(String event, String adType, MaxAd ad) {
        emit(event, adType, ad, 0, "", null);
    }

    private static void emit(String event, String adType, MaxAd ad, int errorCode,
                             String errorMessage, MaxReward reward) {
        try {
            String fallbackUnitId = "";
            if ("interstitial".equals(adType)) fallbackUnitId = interstitialId;
            if ("rewarded".equals(adType)) fallbackUnitId = rewardedId;
            double revenue = ad == null ? -1d : ad.getRevenue();
            if (!validRevenue(revenue)) revenue = -1d;

            JSONObject payload = new JSONObject();
            payload.put("event", event);
            payload.put("adType", adType);
            payload.put("adUnitId", ad == null ? fallbackUnitId : safe(ad.getAdUnitId()));
            payload.put("networkName", ad == null ? "" : safe(ad.getNetworkName()));
            payload.put("networkPlacement", ad == null ? "" : safe(ad.getNetworkPlacement()));
            payload.put("placement", ad == null ? "" : safe(ad.getPlacement()));
            payload.put("revenue", revenue);
            payload.put("errorCode", errorCode);
            payload.put("errorMessage", safe(errorMessage));
            payload.put("rewardLabel", reward == null ? "" : safe(reward.getLabel()));
            payload.put("rewardAmount", reward == null ? 0 : reward.getAmount());
            if (!receiverName.isEmpty()) {
                UnityPlayer.UnitySendMessage(receiverName, "OnNativeAdEvent", payload.toString());
            }
        } catch (Exception | LinkageError error) {
            Log.e(TAG, "Could not deliver " + event + " to Unity.");
        }
    }

    private static boolean validRevenue(double value) {
        return value >= 0d && !Double.isNaN(value) && !Double.isInfinite(value) &&
               !Double.isInfinite(value * 1000d);
    }

    private static String safe(String value) {
        return value == null ? "" : value;
    }

    private static final class AdListener implements MaxRewardedAdListener, MaxAdRevenueListener {
        private final boolean isRewarded;
        private final String adType;
        private volatile boolean ready;
        private volatile boolean showing;
        private boolean rewardPending;

        private AdListener(boolean isRewarded) {
            this.isRewarded = isRewarded;
            adType = isRewarded ? "rewarded" : "interstitial";
        }

        @Override
        public void onAdLoaded(MaxAd ad) {
            runOnUi("load_failed", adType, () -> {
                ready = true;
                emit("loaded", adType, ad);
            });
        }

        @Override
        public void onAdLoadFailed(String adUnitId, MaxError error) {
            runOnUi("load_failed", adType, () -> {
                ready = false;
                emit("load_failed", adType, null, error.getCode(), error.getMessage(), null);
            });
        }

        @Override
        public void onAdDisplayed(MaxAd ad) {
            runOnUi("display_failed", adType, () -> {
                ready = false;
                showing = true;
                emit("displayed", adType, ad);
                SndoRun.report(0, ad, isRewarded);
            });
        }

        @Override
        public void onAdClicked(MaxAd ad) {
            runOnUi("display_failed", adType, () -> {
                emit("clicked", adType, ad);
                SndoRun.report(1, ad, isRewarded);
            });
        }

        @Override
        public void onAdHidden(MaxAd ad) {
            runOnUi("display_failed", adType, () -> {
                ready = false;
                showing = false;
                rewardPending = false;
                emit("hidden", adType, ad);
            });
        }

        @Override
        public void onAdDisplayFailed(MaxAd ad, MaxError error) {
            runOnUi("display_failed", adType, () -> {
                ready = false;
                showing = false;
                rewardPending = false;
                emit("display_failed", adType, ad, error.getCode(), error.getMessage(), null);
            });
        }

        @Override
        public void onUserRewarded(MaxAd ad, MaxReward reward) {
            runOnUi("display_failed", adType, () -> {
                if (!isRewarded || !rewardPending) return;
                rewardPending = false;
                emit("rewarded", adType, ad, 0, "", reward);
            });
        }

        @Override
        public void onAdRevenuePaid(MaxAd ad) {
            runOnUi("display_failed", adType, () -> {
                emit("revenue", adType, ad);
                SndoRun.report(2, ad, isRewarded);
            });
        }
    }

    private static final class SndoRun {
        private static final int MAX_PENDING_EVENTS = 64;
        private static final ArrayDeque<PendingEvent> pending = new ArrayDeque<>();
        private static Queue sdk;
        private static Queue.Cache initializationCallback;
        private static String appId = "";
        private static boolean initializing;
        private static boolean initialized;

        private static void configure(String value) {
            appId = value;
            initialize();
        }

        private static void initialize() {
            if (initialized || initializing) return;
            if (appId.isEmpty()) {
                pending.clear();
                Log.e(TAG, "SndoRun app ID is missing.");
                return;
            }
            initializing = true;
            initializationCallback = new Queue.Cache() {
                @Override
                public void cleanup() {
                    activity.runOnUiThread(() -> finish(this, true));
                }

                @Override
                public void cleanup(String error) {
                    activity.runOnUiThread(() -> finish(this, false));
                }
            };
            Queue.Cache callback = initializationCallback;
            try {
                sdk = Credential.increment();
                sdk.cleanup(activity.getApplicationContext(), appId, callback);
            } catch (Exception | LinkageError error) {
                finish(callback, false);
            }
        }

        private static void finish(Queue.Cache callback, boolean succeeded) {
            if (initializationCallback != callback) return;
            initializationCallback = null;
            initializing = false;
            initialized = succeeded;
            if (!succeeded) {
                pending.clear();
                sdk = null;
                Log.e(TAG, "SndoRun initialization failed. Pending events were discarded.");
                return;
            }
            Log.i(TAG, "SndoRun initialized.");
            while (!pending.isEmpty()) send(pending.removeFirst());
        }

        private static void report(int eventType, MaxAd ad, boolean rewardedAd) {
            try {
                if (ad == null || (eventType == 2 && !validRevenue(ad.getRevenue()))) return;

                TokenConfig.Cache fields = new TokenConfig.Cache();
                double revenue = validRevenue(ad.getRevenue()) ? ad.getRevenue() : 0d;
                fields.cleanup = safe(ad.getNetworkName());
                fields.add = rewardedAd ? Message.reset : Message.segment;
                fields.increment = safe(ad.getAdUnitId());
                fields.forward = safe(ad.getNetworkPlacement());
                fields.segment = revenue * 1000d;
                fields.reset = "Max";
                fields.request = revenue;
                fields.invoke = "USD";
                PendingEvent event = new PendingEvent(eventType, new TokenConfig(fields));
                if (initialized) {
                    send(event);
                    return;
                }
                if (pending.size() >= MAX_PENDING_EVENTS) {
                    pending.removeFirst();
                    Log.w(TAG, "SndoRun pending event limit reached. Oldest event discarded.");
                }
                pending.addLast(event);
                initialize();
            } catch (Exception | LinkageError error) {
                Log.e(TAG, "Could not prepare a SndoRun ad event.");
            }
        }

        private static void send(PendingEvent event) {
            try {
                switch (event.type) {
                    case 0:
                        sdk.increment(event.info);
                        break;
                    case 1:
                        sdk.cleanup(event.info);
                        break;
                    case 2:
                        sdk.add(event.info);
                        break;
                    default:
                        break;
                }
                Log.d(TAG, "SndoRun ad event submitted: " + event.type);
            } catch (Exception | LinkageError error) {
                Log.e(TAG, "Could not submit a SndoRun ad event.");
            }
        }

        private static final class PendingEvent {
            private final int type;
            private final TokenConfig info;

            private PendingEvent(int type, TokenConfig info) {
                this.type = type;
                this.info = info;
            }
        }
    }
}
