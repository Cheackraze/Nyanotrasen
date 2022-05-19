using Content.Shared.CCVar;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared;
using Robust.Shared.Configuration;
using Range = Robust.Client.UserInterface.Controls.Range;

namespace Content.Client.EscapeMenu.UI.Tabs
{
    [GenerateTypedNameReferences]
    public sealed partial class AudioTab : Control
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly IClydeAudio _clydeAudio = default!;

        public AudioTab()
        {
            RobustXamlLoader.Load(this);
            IoCManager.InjectDependencies(this);

            LobbyMusicCheckBox.Pressed = _cfg.GetCVar(CCVars.LobbyMusicEnabled);
            StationAmbienceCheckBox.Pressed = _cfg.GetCVar(CCVars.StationAmbienceEnabled);
            SpaceAmbienceCheckBox.Pressed = _cfg.GetCVar(CCVars.SpaceAmbienceEnabled);

            ApplyButton.OnPressed += OnApplyButtonPressed;
            ResetButton.OnPressed += OnResetButtonPressed;
            MasterVolumeSlider.OnValueChanged += OnMasterVolumeSliderChanged;
            MidiVolumeSlider.OnValueChanged += OnMidiVolumeSliderChanged;
            AmbienceVolumeSlider.OnValueChanged += OnAmbienceVolumeSliderChanged;
            AmbienceSoundsSlider.OnValueChanged += OnAmbienceSoundsSliderChanged;
            LobbyMusicCheckBox.OnToggled += OnLobbyMusicCheckToggled;
            StationAmbienceCheckBox.OnToggled += OnStationAmbienceCheckToggled;
            SpaceAmbienceCheckBox.OnToggled += OnSpaceAmbienceCheckToggled;

            AmbienceSoundsSlider.MinValue = _cfg.GetCVar(CCVars.MinMaxAmbientSourcesConfigured);
            AmbienceSoundsSlider.MaxValue = _cfg.GetCVar(CCVars.MaxMaxAmbientSourcesConfigured);

            Reset();
        }

        protected override void Dispose(bool disposing)
        {
            ApplyButton.OnPressed -= OnApplyButtonPressed;
            ResetButton.OnPressed -= OnResetButtonPressed;
            MasterVolumeSlider.OnValueChanged -= OnMasterVolumeSliderChanged;
            MidiVolumeSlider.OnValueChanged -= OnMidiVolumeSliderChanged;
            AmbienceVolumeSlider.OnValueChanged -= OnAmbienceVolumeSliderChanged;
            base.Dispose(disposing);
        }

        private void OnAmbienceVolumeSliderChanged(Range obj)
        {
            UpdateChanges();
        }

        private void OnAmbienceSoundsSliderChanged(Range obj)
        {
            UpdateChanges();
        }

        private void OnMasterVolumeSliderChanged(Range range)
        {
            _clydeAudio.SetMasterVolume(MasterVolumeSlider.Value / 100);
            UpdateChanges();
        }

        private void OnMidiVolumeSliderChanged(Range range)
        {
            UpdateChanges();
        }

        private void OnLobbyMusicCheckToggled(BaseButton.ButtonEventArgs args)
        {
            UpdateChanges();
        }

        private void OnStationAmbienceCheckToggled(BaseButton.ButtonEventArgs args)
        {
            UpdateChanges();
        }

        private void OnSpaceAmbienceCheckToggled(BaseButton.ButtonEventArgs args)
        {
            UpdateChanges();
        }

        private void OnApplyButtonPressed(BaseButton.ButtonEventArgs args)
        {
            _cfg.SetCVar(CVars.AudioMasterVolume, MasterVolumeSlider.Value / 100);
            _cfg.SetCVar(CVars.MidiVolume, LV100ToDB(MidiVolumeSlider.Value));
            _cfg.SetCVar(CCVars.AmbienceVolume, LV100ToDB(AmbienceVolumeSlider.Value));
            _cfg.SetCVar(CCVars.MaxAmbientSources, (int)AmbienceSoundsSlider.Value);
            _cfg.SetCVar(CCVars.LobbyMusicEnabled, LobbyMusicCheckBox.Pressed);
            _cfg.SetCVar(CCVars.StationAmbienceEnabled, StationAmbienceCheckBox.Pressed);
            _cfg.SetCVar(CCVars.SpaceAmbienceEnabled, SpaceAmbienceCheckBox.Pressed);
            _cfg.SaveToFile();
            UpdateChanges();
        }

        private void OnResetButtonPressed(BaseButton.ButtonEventArgs args)
        {
            Reset();
        }

        private void Reset()
        {
            MasterVolumeSlider.Value = _cfg.GetCVar(CVars.AudioMasterVolume) * 100;
            MidiVolumeSlider.Value = DBToLV100(_cfg.GetCVar(CVars.MidiVolume));
            AmbienceVolumeSlider.Value = DBToLV100(_cfg.GetCVar(CCVars.AmbienceVolume));
            AmbienceSoundsSlider.Value = _cfg.GetCVar(CCVars.MaxAmbientSources);
            LobbyMusicCheckBox.Pressed = _cfg.GetCVar(CCVars.LobbyMusicEnabled);
            StationAmbienceCheckBox.Pressed = _cfg.GetCVar(CCVars.StationAmbienceEnabled);
            SpaceAmbienceCheckBox.Pressed = _cfg.GetCVar(CCVars.SpaceAmbienceEnabled);
            UpdateChanges();
        }

        // Note: Rather than moving these functions somewhere, instead switch MidiManager to using linear units rather than dB
        // Do be sure to rename the setting though
        private float DBToLV100(float db)
        {
            return (MathF.Pow(10, (db / 10)) * 100);
        }

        private float LV100ToDB(float lv100)
        {
            // Saving negative infinity doesn't work, so use -10000000 instead (MidiManager does it)
            return MathF.Max(-10000000, MathF.Log(lv100 / 100, 10) * 10);
        }

        private void UpdateChanges()
        {
            var isMasterVolumeSame =
                Math.Abs(MasterVolumeSlider.Value - _cfg.GetCVar(CVars.AudioMasterVolume) * 100) < 0.01f;
            var isMidiVolumeSame =
                Math.Abs(MidiVolumeSlider.Value - DBToLV100(_cfg.GetCVar(CVars.MidiVolume))) < 0.01f;
            var isAmbientVolumeSame =
                Math.Abs(AmbienceVolumeSlider.Value - DBToLV100(_cfg.GetCVar(CCVars.AmbienceVolume))) < 0.01f;
            var isAmbientSoundsSame = (int)AmbienceSoundsSlider.Value == _cfg.GetCVar(CCVars.MaxAmbientSources);
            var isLobbySame = LobbyMusicCheckBox.Pressed == _cfg.GetCVar(CCVars.LobbyMusicEnabled);
            var isStationAmbienceSame = StationAmbienceCheckBox.Pressed == _cfg.GetCVar(CCVars.StationAmbienceEnabled);
            var isSpaceAmbienceSame = SpaceAmbienceCheckBox.Pressed == _cfg.GetCVar(CCVars.SpaceAmbienceEnabled);
            var isEverythingSame = isMasterVolumeSame && isMidiVolumeSame && isAmbientVolumeSame && isAmbientSoundsSame && isLobbySame && isStationAmbienceSame && isSpaceAmbienceSame;
            ApplyButton.Disabled = isEverythingSame;
            ResetButton.Disabled = isEverythingSame;
            MasterVolumeLabel.Text =
                Loc.GetString("ui-options-volume-percent", ("volume", MasterVolumeSlider.Value / 100));
            MidiVolumeLabel.Text =
                Loc.GetString("ui-options-volume-percent", ("volume", MidiVolumeSlider.Value / 100));
            AmbienceVolumeLabel.Text =
                Loc.GetString("ui-options-volume-percent", ("volume", AmbienceVolumeSlider.Value / 100));
            AmbienceSoundsLabel.Text = ((int)AmbienceSoundsSlider.Value).ToString();
        }
    }
}
