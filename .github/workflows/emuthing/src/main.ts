import * as core from '@actions/core';
import * as exec from '@actions/exec';

async function waitForDevice(port: number, emulatorBootTimeout: number): Promise<void> {
  let booted = false;
  let attempts = 0;
  const retryInterval = 2; // retry every 2 seconds
  const maxAttempts = emulatorBootTimeout / 2;
  while (!booted) {
    try {
      let result = '';
      await exec.exec(`${process.env.ANDROID_HOME}/platform-tools/adb -s emulator-${port} shell getprop sys.boot_completed`, [], {
        listeners: {
          stdout: (data: Buffer) => {
            result += data.toString();
          },
        },
      });
      if (result.trim() === '1') {
        console.log('Emulator booted.');
        booted = true;
        break;
      }
    } catch (error) {
      console.warn(error instanceof Error ? error.message : error);
    }

    if (attempts < maxAttempts) {
      await delay(retryInterval * 1000);
    } else {
      throw new Error(`Timeout waiting for emulator to boot.`);
    }
    attempts++;
  }
}

function delay(ms: number) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

async function run() {
  try {
    // console.log(`Install Android SDK`);
    // const isOnMac = process.platform === 'darwin';
    // const isArm = process.arch === 'arm64';

    // if (!isOnMac) {
    //   await exec.exec(`sh -c \\"sudo chown $USER:$USER ${process.env.ANDROID_HOME} -R`);
    // }

    // const cmdlineToolsPath = `${process.env.ANDROID_HOME}/cmdline-tools`;

    // // add paths for commandline-tools and platform-tools
    // core.addPath(`${cmdlineToolsPath}/latest:${cmdlineToolsPath}/latest/bin:${process.env.ANDROID_HOME}/platform-tools`);

    // set standard AVD path
    // core.exportVariable('ANDROID_AVD_HOME', `${process.env.HOME}/.android/avd`);

    // console.log(`Creating AVD.`);
    
    // await exec.exec(`sh -c \\"set"`);
    // await exec.exec(
    //   `sh -c \\"echo no | ${process.env.ANDROID_HOME}/cmdline-tools/latest/bin/avdmanager create avd --force -n "test" --abi 'google_apis/x86_64' --package 'system-images;android-34;google_apis;x86_64' "`
    // );

    // await exec.exec(`sh -c \\"ls -l /home/runner/.android"`);
    // await exec.exec(`sh -c \\"ls -l /home/runner/.android/avd"`);
    // await exec.exec(`sh -c \\"ls -l /home/runner/.android/avd/test.avd"`);
    // await exec.exec(`sh -c \\"cat /home/runner/.android/avd/test.avd/config.ini"`);

    // await exec.exec(`sh -c \\"printf 'hw.cpu.ncore=2\n' >> /home/runner/.android/avd/config.ini`);

    // console.log('Starting emulator.');

    // await exec.exec(`sh -c \\"${process.env.ANDROID_HOME}/emulator/emulator -port 5554 -avd test -no-window -gpu swiftshader_indirect -no-snapshot -noaudio -no-boot-anim &"`);

    await waitForDevice(5554, 600);
  } finally {
    await exec.exec(`${process.env.ANDROID_HOME}/platform-tools/adb -s emulator-5554 emu kill`);
  }
}

run();
