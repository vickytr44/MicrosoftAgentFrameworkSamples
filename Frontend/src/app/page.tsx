"use client";

import { ProverbsCard } from "@/components/proverbs";
import { WeatherCard } from "@/components/weather";
import { MoonCard } from "@/components/moon";
import HumanApprovalCard from "@/components/humanApprovalCard";
import DeleteConfirmationCard from "@/components/deleteConfirmationCard";
import { AgentState } from "@/lib/types";
import {
  useCoAgent,
  useDefaultTool,
  useFrontendTool,
  useHumanInTheLoop,
  useRenderToolCall,
} from "@copilotkit/react-core";
import { CopilotKitCSSProperties, CopilotSidebar } from "@copilotkit/react-ui";
import { useState } from "react";
import { DynamicForm, Parameter } from "@/components/dynamicForm";

export default function CopilotKitPage() {
  const [themeColor, setThemeColor] = useState("#6366f1");

  useFrontendTool({
    name: "setThemeColor",
    parameters: [
      {
        name: "themeColor",
        description: "The theme color to set. Make sure to pick nice colors.",
        required: true,
      },
    ],
    handler({ themeColor }) {
      setThemeColor(themeColor);
    },
  });

  return (
    <main
      style={
        { "--copilot-kit-primary-color": themeColor } as CopilotKitCSSProperties
      }
    >
      <CopilotSidebar
        disableSystemMessage={true}
        clickOutsideToClose={false}
        labels={{
          title: "Popup Assistant",
          initial: "👋 Hi, there! You're chatting with an agent.",
        }}
        suggestions={[
          {
            title: "Generative UI",
            message: "fill out the user form.",
          },
          {
            title: "Frontend Tools",
            message: "Set the theme to green.",
          },
          {
            title: "Human In the Loop",
            message: "Please go to the moon.",
          },
          {
            title: "Write Agent State",
            message: "Add a proverb about AI.",
          },
          {
            title: "Update Agent State",
            message:
              "Please remove 1 random proverb from the list if there are any.",
          },
          {
            title: "Read Agent State",
            message: "What are the proverbs?",
          },
        ]}
      >
        <YourMainContent themeColor={themeColor} />
      </CopilotSidebar>
    </main>
  );
}

function fetchParametersToFillForm() {
  return [
    {
      name: "message",
      description: "The message to display in the alert.",
      required: true,
    },
    {
      name: "title",
      description: "The title to display in the alert.",
      required: true,
    },
  ];
}

function YourMainContent({ themeColor }: { themeColor: string }) {
  // 🪁 Shared State: https://docs.copilotkit.ai/pydantic-ai/shared-state
  const { state, setState } = useCoAgent<AgentState>({
    name: "my_agent",
    initialState: {
      proverbs: [
        "CopilotKit may be new, but its the best thing since sliced bread.",
      ],
    },
  });

  //🪁 Generative UI: https://docs.copilotkit.ai/pydantic-ai/generative-ui
  useRenderToolCall(
    {
      name: "get_weather",
      description: "Get the weather for a given location.",
      parameters: [{ name: "location", type: "string", required: true }],
      render: ({ args, result }) => {
        return <WeatherCard location={args.location} themeColor={themeColor} />;
      },
    },
    [themeColor],
  );


  // 🪁 Human In the Loop: https://docs.copilotkit.ai/pydantic-ai/human-in-the-loop
  useHumanInTheLoop(
    {
      name: "go_to_moon",
      description: "Go to the moon on request.",
      render: ({ respond, status }) => {
        return (
          <MoonCard themeColor={themeColor} status={status} respond={respond} />
        );
      },
    },
    [themeColor],
  );

  //   useHumanInTheLoop(
  //   {
  //     name: "fill_form_to_alert",
  //     description: "Fill out the alert form on request.",
  //     render: ({ respond, status }) => {
  //       return (
  //         <DynamicForm
  //         parameters = {fetchParametersToFillForm()}
  //         onSubmit={(values) =>{
  //           console.log(values);
  //           alert(`${values.title}: ${values.message}`)
  //         } 
  //       }
  //       />
  //       );
  //     },
  //   },
  //   [themeColor],
  // );

  useHumanInTheLoop({
    name: "humanApprovedCommand",
    description: "Ask human for approval to run a command.",
    parameters: [
      {
        name: "command",
        type: "string",
        description: "The command to run",
        required: true,
      },
    ],
    render: ({ args, respond }) => {
      return <HumanApprovalCard args={args as any} respond={respond as any} />;
    },
  });

  useHumanInTheLoop(
    {
      name: "confirmDelete",
      description: "Ask the human to confirm deletion of an item.",
      parameters: [
        {
          name: "item",
          type: "string",
          description: "The name of the item to delete",
          required: true,
        },
      ],
      render: ({ args, respond }) => {
        return <DeleteConfirmationCard args={args as any} respond={respond as any} />;
      },
    },
    [themeColor],
  );

  return (
    <div
      style={{ backgroundColor: themeColor }}
      className="h-screen flex justify-center items-center flex-col transition-colors duration-300"
    >
      <ProverbsCard state={state} setState={setState} />
    </div>
  );
}
